using UnityEngine;
using System.Collections.Generic;

// Вешается на префаб газона. Строит меш из сетки вершин (каждая поднимается
// по террейну, поэтому газон ложится на склон), расставляет травинки
// с заданной плотностью. Умеет растягиваться за грань.
//
// Грань определяется по реальной точке попадания луча в BoxCollider
// (ближайшая грань в локальных X/Z).
//
// ВАЖНО: во время растягивания работаем в ЛОКАЛЬНЫХ осях газона,
// зафиксировав его позицию и rotation на момент старта. Это позволяет
// корректно растягивать газон после любого поворота вокруг Y -
// поворот не влияет на расчёт.
//
// Во время растягивания:
//   - transform.position.y фиксирован, чтобы плоскость газона не "дышала"
//     по высоте и точка под лучом не скакала;
//   - для расчёта новой длины/ширины луч математически пересекается
//     с плоскостью газона (она горизонтальная, т.к. поворот только по Y);
//   - якорь (противоположная грань) фиксирован в локальных координатах,
//     двигается только длина или ширина по той оси, по которой тянем.
//
// После отпускания Grip:
//   - transform.position.y пересчитывается по террейну один раз,
//     трава пересоздаётся.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class GazonTile : MonoBehaviour
{
    public enum GranRastyagivaniya { Net, Pered, Zad, Levo, Pravo }

    [Header("Размеры тайла в метрах")]
    public float dlina = 1f;
    public float shirina = 1f;

    [Header("Минимальный и максимальный размеры (метры)")]
    public float minRazmer = 0.3f;
    public float maxRazmer = 10f;

    [Header("Плотность травинок на квадратный метр")]
    public float plotnost = 7f;

    [Header("Префаб травинки")]
    public GameObject prefabTravinki;

    [Header("Слой террейна")]
    public LayerMask maskaTerreina;

    [Header("Контейнер для травинок (создаётся автоматически)")]
    public Transform kontejnerTravinok;

    [Header("Сколько подквадратов по каждой оси (4 = 4x4, точнее = больше)")]
    public int podrazbienie = 4;

    [Header("Запас по высоте для BoxCollider (для удобного raycast)")]
    public float vysotaKollaydera = 0.4f;

    private Mesh mesh;
    private BoxCollider boxCollider;
    private List<GameObject> travinki = new List<GameObject>();

    private bool rastyagivaem = false;
    private GranRastyagivaniya gran = GranRastyagivaniya.Net;

    // Состояние, зафиксированное на момент старта растягивания.
    private Vector3 ishodnayaPozitsiya;
    private Quaternion ishodnyyRotation;
    private float shirinaSto;
    private float dlinaSto;

    // Якорь - координата неподвижной грани в локальных осях газона.
    private float yakorLokalnyyX;
    private float yakorLokalnyyZ;

    public bool Rastyagivaetsya
    {
        get { return rastyagivaem; }
    }

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "GazonMesh";
        GetComponent<MeshFilter>().mesh = mesh;
        boxCollider = GetComponent<BoxCollider>();
    }

    void Start()
    {
        Postroit();
    }

    public void Rastyanut(float novayaDlina, float novayaShirina)
    {
        dlina = Mathf.Clamp(novayaDlina, minRazmer, maxRazmer);
        shirina = Mathf.Clamp(novayaShirina, minRazmer, maxRazmer);
        Postroit();
    }

    public void Postroit()
    {
        PostroitMesh();
        ObnovitKollayder();
        RassaditTravinki();
    }

    public void PerestroiMeshIKollayder()
    {
        PostroitMesh();
        ObnovitKollayder();
    }

    // Пересечение луча с горизонтальной плоскостью газона на высоте transform.position.y.
    public Vector3 ProektirovatLuchNaPloskost(Vector3 nachalo, Vector3 napravlenie)
    {
        if (Mathf.Abs(napravlenie.y) < 0.001f)
        {
            return nachalo;
        }

        float t = (transform.position.y - nachalo.y) / napravlenie.y;
        if (t < 0f)
        {
            return nachalo;
        }

        return nachalo + t * napravlenie;
    }

    // ===== Растягивание =====

    // tochkaPopadaniyaMirovaya - реальная точка попадания луча в BoxCollider.
    // По ней определяется грань (ближайшая к точке в локальных X/Z).
    public bool NachatRastyagivanie(Vector3 tochkaPopadaniyaMirovaya)
    {
        Vector3 mestnaya = transform.InverseTransformPoint(tochkaPopadaniyaMirovaya);

        float doLevoy = mestnaya.x - (-shirina / 2f);
        float doPravoy = shirina / 2f - mestnaya.x;
        float doPeredney = mestnaya.z - (-dlina / 2f);
        float doZadney = dlina / 2f - mestnaya.z;

        float minDist = doLevoy;
        gran = GranRastyagivaniya.Levo;

        if (doPravoy < minDist) { minDist = doPravoy; gran = GranRastyagivaniya.Pravo; }
        if (doPeredney < minDist) { minDist = doPeredney; gran = GranRastyagivaniya.Pered; }
        if (doZadney < minDist) { minDist = doZadney; gran = GranRastyagivaniya.Zad; }

        // Фиксируем позицию, rotation и размеры на момент старта.
        ishodnayaPozitsiya = transform.position;
        ishodnyyRotation = transform.rotation;
        shirinaSto = shirina;
        dlinaSto = dlina;

        // Якорь - координата НЕПОДВИЖНОЙ (противоположной) грани
        // в локальных осях газона на момент старта.
        // Локальные оси: передняя = -dlinaSto/2, задняя = +dlinaSto/2,
        // левая = -shirinaSto/2, правая = +shirinaSto/2.
        if (gran == GranRastyagivaniya.Pravo)
        {
            yakorLokalnyyX = -shirinaSto / 2f; // неподвижна левая
        }
        else if (gran == GranRastyagivaniya.Levo)
        {
            yakorLokalnyyX = shirinaSto / 2f; // неподвижна правая
        }
        else if (gran == GranRastyagivaniya.Pered)
        {
            yakorLokalnyyZ = dlinaSto / 2f; // неподвижна задняя
        }
        else if (gran == GranRastyagivaniya.Zad)
        {
            yakorLokalnyyZ = -dlinaSto / 2f; // неподвижна передняя
        }

        if (kontejnerTravinok != null)
        {
            kontejnerTravinok.gameObject.SetActive(false);
        }

        rastyagivaem = true;
        return true;
    }

    public void ObnovitRastyagivanie(Vector3 nachaloLucha, Vector3 napravlenieLucha)
    {
        if (!rastyagivaem) return;

        // Мировая точка под лучом на горизонтальной плоскости газона.
        Vector3 toMirovaya = ProektirovatLuchNaPloskost(nachaloLucha, napravlenieLucha);

        // Переводим в локальные оси газона ОТНОСИТЕЛЬНО исходной позиции
        // и исходного rotation. Это ключ: не зависим от поворотов и от
        // текущего transform.position (он будет смещаться по X/Z).
        Vector3 smeshchenie = toMirovaya - ishodnayaPozitsiya;
        Vector3 lokalnaya = Quaternion.Inverse(ishodnyyRotation) * smeshchenie;

        // Новый локальный центр газона (Y = 0, X/Z - по грани).
        Vector3 novyyLokalnyyCentr = Vector3.zero;

        switch (gran)
        {
            case GranRastyagivaniya.Pravo:
                shirina = Mathf.Clamp(lokalnaya.x - yakorLokalnyyX, minRazmer, maxRazmer);
                novyyLokalnyyCentr = new Vector3(yakorLokalnyyX + shirina / 2f, 0f, 0f);
                break;

            case GranRastyagivaniya.Levo:
                shirina = Mathf.Clamp(yakorLokalnyyX - lokalnaya.x, minRazmer, maxRazmer);
                novyyLokalnyyCentr = new Vector3(yakorLokalnyyX - shirina / 2f, 0f, 0f);
                break;

            case GranRastyagivaniya.Pered:
                dlina = Mathf.Clamp(yakorLokalnyyZ - lokalnaya.z, minRazmer, maxRazmer);
                novyyLokalnyyCentr = new Vector3(0f, 0f, yakorLokalnyyZ - dlina / 2f);
                break;

            case GranRastyagivaniya.Zad:
                dlina = Mathf.Clamp(lokalnaya.z - yakorLokalnyyZ, minRazmer, maxRazmer);
                novyyLokalnyyCentr = new Vector3(0f, 0f, yakorLokalnyyZ + dlina / 2f);
                break;
        }

        // Мировая позиция нового центра. Y фиксирован (исходный),
        // чтобы газон не "дышал" по высоте во время растягивания.
        Vector3 novayaMirovayaPozitsiya = ishodnayaPozitsiya + ishodnyyRotation * novyyLokalnyyCentr;
        novayaMirovayaPozitsiya.y = ishodnayaPozitsiya.y;

        transform.position = novayaMirovayaPozitsiya;

        PostroitMesh();
        ObnovitKollayder();
    }

    public void ZakonchitRastyagivanie()
    {
        if (!rastyagivaem) return;

        rastyagivaem = false;
        gran = GranRastyagivaniya.Net;

        // Прилипаем к террейну по центру один раз, после завершения.
        Vector3 centr = transform.position;
        float vysotaCentra = PoluchitVisotuTerreina(centr);
        transform.position = new Vector3(centr.x, vysotaCentra, centr.z);

        if (kontejnerTravinok != null)
        {
            kontejnerTravinok.gameObject.SetActive(true);
        }

        PostroitMesh();
        ObnovitKollayder();
        RassaditTravinki();
    }

    // ===== Построение =====

    private void PostroitMesh()
    {
        mesh.Clear();

        int subdiv = Mathf.Max(1, podrazbienie);
        int vershin = subdiv + 1;
        Vector3[] verts = new Vector3[vershin * vershin];

        for (int iz = 0; iz < vershin; iz++)
        {
            for (int ix = 0; ix < vershin; ix++)
            {
                float tx = (float)ix / subdiv;
                float tz = (float)iz / subdiv;
                float lx = Mathf.Lerp(-shirina / 2f, shirina / 2f, tx);
                float lz = Mathf.Lerp(-dlina / 2f, dlina / 2f, tz);
                Vector3 mestnaya = new Vector3(lx, 0, lz);
                Vector3 mirovaya = transform.TransformPoint(mestnaya);
                mirovaya.y = PoluchitVisotuTerreina(mirovaya);
                verts[iz * vershin + ix] = transform.InverseTransformPoint(mirovaya);
            }
        }

        int[] triangles = new int[subdiv * subdiv * 6];
        int idx = 0;
        for (int iz = 0; iz < subdiv; iz++)
        {
            for (int ix = 0; ix < subdiv; ix++)
            {
                int v0 = iz * vershin + ix;
                int v1 = v0 + 1;
                int v2 = v0 + vershin;
                int v3 = v2 + 1;

                triangles[idx++] = v0;
                triangles[idx++] = v2;
                triangles[idx++] = v1;
                triangles[idx++] = v1;
                triangles[idx++] = v2;
                triangles[idx++] = v3;
            }
        }

        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void ObnovitKollayder()
    {
        if (boxCollider == null) return;

        Bounds b = mesh.bounds;

        boxCollider.center = new Vector3(b.center.x, b.center.y + vysotaKollaydera / 2f, b.center.z);
        boxCollider.size = new Vector3(
            Mathf.Max(b.size.x, 0.1f),
            Mathf.Max(b.size.y + vysotaKollaydera, vysotaKollaydera),
            Mathf.Max(b.size.z, 0.1f)
        );
    }

    private float PoluchitVisotuTerreina(Vector3 mirovaya)
    {
        RaycastHit hit;
        if (Physics.Raycast(mirovaya + Vector3.up * 100f, Vector3.down, out hit, 200f, maskaTerreina))
        {
            return hit.point.y;
        }
        return mirovaya.y;
    }

    private void RassaditTravinki()
    {
        if (prefabTravinki == null) return;

        if (kontejnerTravinok == null)
        {
            GameObject go = new GameObject("Travinki");
            go.transform.SetParent(transform, false);
            kontejnerTravinok = go.transform;
        }

        foreach (var t in travinki)
        {
            if (t != null) Destroy(t);
        }
        travinki.Clear();

        int kolichestvo = Mathf.RoundToInt(plotnost * dlina * shirina);

        for (int i = 0; i < kolichestvo; i++)
        {
            float x = Random.Range(-shirina / 2f, shirina / 2f);
            float z = Random.Range(-dlina / 2f, dlina / 2f);
            Vector3 mestnaya = new Vector3(x, 0, z);
            Vector3 mirovaya = transform.TransformPoint(mestnaya);
            mirovaya.y = PoluchitVisotuTerreina(mirovaya);
            GameObject travinka = Instantiate(prefabTravinki, mirovaya, Quaternion.identity, kontejnerTravinok);
            travinki.Add(travinka);
        }

        PodsvetkaNasazhdeniya podsvetka = GetComponent<PodsvetkaNasazhdeniya>();
        if (podsvetka != null)
        {
            podsvetka.ObnovitPodsvetku();
        }
    }
}