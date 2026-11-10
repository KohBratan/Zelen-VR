using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class SekciyaNasazhdeniy : MonoBehaviour
{
    public enum GranRastyagivaniya { Net, Pered, Zad, Levo, Pravo }

    [Header("Контейнеры внутри префаба (положи туда по 1 объекту для шаблона)")]
    public Transform containerDerevya;
    public Transform containerKusty;

    [Header("Шаг сетки (расстояние между объектами)")]
    public float shagX = 2f;
    public float shagZ = 2f;

    [Header("Текущие размеры секции")]
    public float shirina = 4f;
    public float dlinaDerevya = 2f;
    public float dlinaKusty = 2f;

    [Header("Минимальные размеры")]
    public float minShirina = 2f;
    public float minDlina = 1f;

    [Header("Слой террейна")]
    public LayerMask maskaTerreina;

    [Header("Сколько подквадратов по каждой оси (для подложки)")]
    public int podrazbienie = 4;

    [Header("Толщина коллайдера (тонкий, чтобы можно было ходить)")]
    public float vysotaKollaydera = 0.03f;

    // Флаг для призрака (предпросмотра)
    public bool etoPrizrak = false;

    private Mesh mesh;
    private BoxCollider boxCollider;

    private GameObject prefabDerevo;
    private GameObject prefabKust;
    private List<GameObject> spisokDerevya = new List<GameObject>();
    private List<GameObject> spisokKusty = new List<GameObject>();

    private bool rastyagivaem = false;
    private GranRastyagivaniya gran = GranRastyagivaniya.Net;

    private Vector3 ishodnayaPozitsiya;
    private Quaternion ishodnyyRotation;
    private float yakorLokalnyyX;

    public bool Rastyagivaetsya => rastyagivaem;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "SekciyaMesh";
        GetComponent<MeshFilter>().mesh = mesh;
        boxCollider = GetComponent<BoxCollider>();

        if (containerDerevya != null && containerDerevya.childCount > 0)
        {
            prefabDerevo = containerDerevya.GetChild(0).gameObject;
            prefabDerevo.SetActive(false);
            Collider c = prefabDerevo.GetComponent<Collider>();
            if (c != null) c.enabled = false;

            for (int i = containerDerevya.childCount - 1; i >= 1; i--)
                Destroy(containerDerevya.GetChild(i).gameObject);
        }

        if (containerKusty != null && containerKusty.childCount > 0)
        {
            prefabKust = containerKusty.GetChild(0).gameObject;
            prefabKust.SetActive(false);
            Collider c = prefabKust.GetComponent<Collider>();
            if (c != null) c.enabled = false;

            for (int i = containerKusty.childCount - 1; i >= 1; i--)
                Destroy(containerKusty.GetChild(i).gameObject);
        }
    }

    void Start()
    {
        if (etoPrizrak)
        {
            // Если это призрак - строим только подложку, растения не спавним
            PostroitMesh();
            ObnovitKollayder();
        }
        else
        {
            Postroit();
        }
    }

    public void Postroit()
    {
        PostroitMesh();
        ObnovitKollayder();
        RassaditObekty();
    }

    public void PerestroiMeshIKollayder()
    {
        PostroitMesh();
        ObnovitKollayder();
    }

    private float PoluchitVisotuTerreina(Vector3 mirovaya)
    {
        RaycastHit hit;
        if (Physics.Raycast(mirovaya + Vector3.up * 100f, Vector3.down, out hit, 200f, maskaTerreina))
            return hit.point.y;
        return mirovaya.y;
    }

    private void PostroitMesh()
    {
        mesh.Clear();

        float obshayaDlina = dlinaDerevya + dlinaKusty;
        float centerZ = (dlinaDerevya - dlinaKusty) / 2f;

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
                float lz = Mathf.Lerp(-obshayaDlina / 2f, obshayaDlina / 2f, tz);

                Vector3 mestnaya = new Vector3(lx, 0, lz + centerZ);
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

    private void RassaditObekty()
    {
        if (prefabDerevo == null || prefabKust == null || etoPrizrak) return;

        spisokDerevya.RemoveAll(item => item == null);
        spisokKusty.RemoveAll(item => item == null);

        int kolvoStolbtsov = Mathf.Max(1, Mathf.FloorToInt(shirina / shagX) + 1);
        int kolvoRyadovDerevya = Mathf.Max(1, Mathf.FloorToInt(dlinaDerevya / shagZ) + 1);
        int kolvoRyadovKusty = Mathf.Max(1, Mathf.FloorToInt(dlinaKusty / shagZ) + 1);

        int totalDerevya = kolvoStolbtsov * kolvoRyadovDerevya;
        int totalKusty = kolvoStolbtsov * kolvoRyadovKusty;

        ObnovitPul(spisokDerevya, prefabDerevo, totalDerevya, containerDerevya);
        ObnovitPul(spisokKusty, prefabKust, totalKusty, containerKusty);

        float startX = -(kolvoStolbtsov - 1) * shagX / 2f;

        float stepZDerevya = dlinaDerevya / kolvoRyadovDerevya;
        float stepZKusty = dlinaKusty / kolvoRyadovKusty;

        for (int col = 0; col < kolvoStolbtsov; col++)
        {
            float x = startX + col * shagX;

            // Деревья
            int treeIndex = col * kolvoRyadovDerevya;
            for (int row = 0; row < kolvoRyadovDerevya; row++)
            {
                float z = (row + 0.5f) * stepZDerevya;
                Vector3 localPos = new Vector3(x, 0, z);
                Vector3 worldPos = transform.TransformPoint(localPos);

                worldPos.y = PoluchitVisotuTerreina(worldPos);
                spisokDerevya[treeIndex + row].transform.position = worldPos;
            }

            // Кусты
            int bushIndex = col * kolvoRyadovKusty;
            for (int row = 0; row < kolvoRyadovKusty; row++)
            {
                float z = -dlinaKusty + (row + 0.5f) * stepZKusty;
                Vector3 localPos = new Vector3(x, 0, z);
                Vector3 worldPos = transform.TransformPoint(localPos);

                worldPos.y = PoluchitVisotuTerreina(worldPos);
                spisokKusty[bushIndex + row].transform.position = worldPos;
            }
        }

        PodsvetkaNasazhdeniya podsvetka = GetComponent<PodsvetkaNasazhdeniya>();
        if (podsvetka != null) podsvetka.ObnovitPodsvetku();
    }

    private void ObnovitPul(List<GameObject> pul, GameObject prefab, int target, Transform parent)
    {
        while (pul.Count < target)
        {
            GameObject novoe = Instantiate(prefab, parent);
            novoe.SetActive(true);
            novoe.name = prefab.name + "_" + pul.Count;

            // ВКЛЮЧАЕМ КОЛЛАЙДЕРЫ У СОЗДАННЫХ КОПИЙ
            Collider[] colls = novoe.GetComponentsInChildren<Collider>(true);
            foreach (var c in colls) c.enabled = true;

            pul.Add(novoe);
        }
        while (pul.Count > target)
        {
            Destroy(pul[pul.Count - 1]);
            pul.RemoveAt(pul.Count - 1);
        }
    }

    public Vector3 ProektirovatLuchNaPloskost(Vector3 nachalo, Vector3 napravlenie)
    {
        if (Mathf.Abs(napravlenie.y) < 0.001f) return nachalo;
        float t = (transform.position.y - nachalo.y) / napravlenie.y;
        return (t < 0f) ? nachalo : nachalo + t * napravlenie;
    }

    public bool NachatRastyagivanie(Vector3 tochkaMirovaya)
    {
        Vector3 mestnaya = transform.InverseTransformPoint(tochkaMirovaya);

        float doPravoy = shirina / 2f - mestnaya.x;
        float doLevoy = mestnaya.x - (-shirina / 2f);
        float doPeredney = mestnaya.z - (-dlinaKusty);
        float doZadney = dlinaDerevya - mestnaya.z;

        float minDist = doLevoy;
        gran = GranRastyagivaniya.Levo;

        if (doPravoy < minDist) { minDist = doPravoy; gran = GranRastyagivaniya.Pravo; }
        if (doPeredney < minDist) { minDist = doPeredney; gran = GranRastyagivaniya.Pered; }
        if (doZadney < minDist) { minDist = doZadney; gran = GranRastyagivaniya.Zad; }

        ishodnayaPozitsiya = transform.position;
        ishodnyyRotation = transform.rotation;

        if (gran == GranRastyagivaniya.Pravo) yakorLokalnyyX = -shirina / 2f;
        else if (gran == GranRastyagivaniya.Levo) yakorLokalnyyX = shirina / 2f;

        rastyagivaem = true;
        return true;
    }

    public void ObnovitRastyagivanie(Vector3 nachaloLucha, Vector3 napravlenieLucha)
    {
        if (!rastyagivaem) return;

        Vector3 toMirovaya = ProektirovatLuchNaPloskost(nachaloLucha, napravlenieLucha);
        Vector3 smeshchenie = toMirovaya - ishodnayaPozitsiya;
        Vector3 lokalnaya = Quaternion.Inverse(ishodnyyRotation) * smeshchenie;
        Vector3 novyyLokalnyyCentr = Vector3.zero;

        switch (gran)
        {
            case GranRastyagivaniya.Pravo:
                shirina = Mathf.Clamp(lokalnaya.x - yakorLokalnyyX, minShirina, 50f);
                novyyLokalnyyCentr = new Vector3(yakorLokalnyyX + shirina / 2f, 0f, 0f);
                break;
            case GranRastyagivaniya.Levo:
                shirina = Mathf.Clamp(yakorLokalnyyX - lokalnaya.x, minShirina, 50f);
                novyyLokalnyyCentr = new Vector3(yakorLokalnyyX - shirina / 2f, 0f, 0f);
                break;
            case GranRastyagivaniya.Pered:
                dlinaKusty = Mathf.Clamp(-lokalnaya.z, minDlina, 50f);
                novyyLokalnyyCentr = new Vector3(0f, 0f, 0f);
                break;
            case GranRastyagivaniya.Zad:
                dlinaDerevya = Mathf.Clamp(lokalnaya.z, minDlina, 50f);
                novyyLokalnyyCentr = new Vector3(0f, 0f, 0f);
                break;
        }

        Vector3 novayaMirovayaPozitsiya = ishodnayaPozitsiya + ishodnyyRotation * novyyLokalnyyCentr;
        novayaMirovayaPozitsiya.y = ishodnayaPozitsiya.y;
        transform.position = novayaMirovayaPozitsiya;

        PostroitMesh();
        ObnovitKollayder();
        RassaditObekty();
    }

    public void ZakonchitRastyagivanie()
    {
        if (!rastyagivaem) return;
        rastyagivaem = false;
        gran = GranRastyagivaniya.Net;

        Vector3 centr = transform.position;
        float vysotaCentra = PoluchitVisotuTerreina(centr);
        transform.position = new Vector3(centr.x, vysotaCentra, centr.z);

        PostroitMesh();
        ObnovitKollayder();
        RassaditObekty();
    }
}