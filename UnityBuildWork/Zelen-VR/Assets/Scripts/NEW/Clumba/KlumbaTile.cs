using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class KlumbaTile : MonoBehaviour
{
    public enum GranRastyagivaniya { Net, Pered, Zad, Levo, Pravo }

    [Header("Размеры клумбы")]
    public float shirina = 1f;
    public float dlina = 1f;
    public float minRazmer = 0.5f;
    public float maxRazmer = 10f;

    [Header("Настройки забора")]
    public GameObject prefabKub;
    public float vysotaZabora = 0.3f;
    public float tolshinaZabora = 0.05f;
    [Tooltip("На сколько опустить забор под землю")]
    public float pogruzhenieVPochvu = 0.15f; // <--- НОВОЕ ПОЛЕ

    [Header("Материалы для смены цвета")]
    public Material[] materialyZabora;
    private int tekushiyTsvetIndex = 0;

    [Header("Контейнер для цветов (создаётся сам)")]
    public Transform kontejnerCvetov;

    [Header("Слой террейна")]
    public LayerMask maskaTerreina;

    [Header("Настройки меша")]
    public int podrazbienie = 4;
    public float vysotaKollaydera = 0.1f;

    private Mesh mesh;
    private BoxCollider boxCollider;
    private bool rastyagivaem = false;
    private GranRastyagivaniya gran = GranRastyagivaniya.Net;

    private Vector3 ishodnayaPozitsiya;
    private Quaternion ishodnyyRotation;
    private float yakorLokalnyyX;
    private float yakorLokalnyyZ;

    private Transform[] steny = new Transform[4];

    public bool Rastyagivaetsya => rastyagivaem;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "KlumbaMesh";
        GetComponent<MeshFilter>().mesh = mesh;
        boxCollider = GetComponent<BoxCollider>();

        if (kontejnerCvetov == null)
        {
            GameObject go = new GameObject("Cvety");
            go.transform.SetParent(transform, false);
            kontejnerCvetov = go.transform;
        }

        if (prefabKub != null)
        {
            GameObject kont = new GameObject("Zabor");
            kont.transform.SetParent(transform, false);

            for (int i = 0; i < 4; i++)
            {
                GameObject stena = Instantiate(prefabKub, kont.transform);
                stena.name = "Stena_" + i;
                steny[i] = stena.transform;
            }
        }
    }

    void Start()
    {
        Postroit();
    }

    public void Postroit()
    {
        PostroitMesh();
        ObnovitKollayder();
        ObnovitZabor();
    }

    private float PoluchitVisotuTerreina(Vector3 mirovaya)
    {
        if (Physics.Raycast(mirovaya + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f, maskaTerreina))
            return hit.point.y;
        return mirovaya.y;
    }

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

                triangles[idx++] = v0; triangles[idx++] = v2; triangles[idx++] = v1;
                triangles[idx++] = v1; triangles[idx++] = v2; triangles[idx++] = v3;
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
        boxCollider.size = new Vector3(Mathf.Max(b.size.x, 0.1f), Mathf.Max(b.size.y + vysotaKollaydera, vysotaKollaydera), Mathf.Max(b.size.z, 0.1f));
    }

    private void ObnovitZabor()
    {
        if (steny[0] == null) return;

        float x = shirina / 2f;
        float z = dlina / 2f;

        // Опускаем забор под землю
        float h = (vysotaZabora / 2f) - pogruzhenieVPochvu;

        steny[0].localPosition = new Vector3(0, h, z);
        steny[0].localScale = new Vector3(shirina, vysotaZabora, tolshinaZabora);

        steny[1].localPosition = new Vector3(0, h, -z);
        steny[1].localScale = new Vector3(shirina, vysotaZabora, tolshinaZabora);

        steny[2].localPosition = new Vector3(x, h, 0);
        steny[2].localScale = new Vector3(tolshinaZabora, vysotaZabora, dlina);

        steny[3].localPosition = new Vector3(-x, h, 0);
        steny[3].localScale = new Vector3(tolshinaZabora, vysotaZabora, dlina);
    }

    private void UdalitCvetyZaGranitsami()
    {
        if (kontejnerCvetov == null) return;

        float halfX = shirina / 2f;
        float halfZ = dlina / 2f;

        List<GameObject> udalit = new List<GameObject>();

        foreach (Transform child in kontejnerCvetov)
        {
            Vector3 localPos = child.localPosition;
            if (Mathf.Abs(localPos.x) > halfX || Mathf.Abs(localPos.z) > halfZ)
            {
                udalit.Add(child.gameObject);
            }
        }

        foreach (var obj in udalit)
        {
            Destroy(obj);
        }
    }

    public void SmenitTsvet()
    {
        if (materialyZabora == null || materialyZabora.Length < 2) return;

        tekushiyTsvetIndex = (tekushiyTsvetIndex + 1) % materialyZabora.Length;
        Material noviyMat = materialyZabora[tekushiyTsvetIndex];

        List<Renderer> rends = new List<Renderer>();
        foreach (var s in steny)
        {
            if (s != null)
            {
                Renderer[] rs = s.GetComponentsInChildren<Renderer>();
                foreach (var r in rs)
                {
                    r.SetPropertyBlock(null);
                    r.material = noviyMat;
                    rends.Add(r);
                }
            }
        }

        PodsvetkaNasazhdeniya podsvetka = GetComponent<PodsvetkaNasazhdeniya>();
        if (podsvetka != null) podsvetka.ObnovitKesh(rends.ToArray(), noviyMat);
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
        float doPeredney = mestnaya.z - (-dlina / 2f);
        float doZadney = dlina / 2f - mestnaya.z;

        float minDist = doLevoy; gran = GranRastyagivaniya.Levo;
        if (doPravoy < minDist) { minDist = doPravoy; gran = GranRastyagivaniya.Pravo; }
        if (doPeredney < minDist) { minDist = doPeredney; gran = GranRastyagivaniya.Pered; }
        if (doZadney < minDist) { minDist = doZadney; gran = GranRastyagivaniya.Zad; }

        ishodnayaPozitsiya = transform.position;
        ishodnyyRotation = transform.rotation;

        if (gran == GranRastyagivaniya.Pravo) yakorLokalnyyX = -shirina / 2f;
        else if (gran == GranRastyagivaniya.Levo) yakorLokalnyyX = shirina / 2f;
        else if (gran == GranRastyagivaniya.Pered) yakorLokalnyyZ = dlina / 2f;
        else if (gran == GranRastyagivaniya.Zad) yakorLokalnyyZ = -dlina / 2f;

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

        Vector3 starayaPozitsiya = transform.position;

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

        Vector3 novayaMirovayaPozitsiya = ishodnayaPozitsiya + ishodnyyRotation * novyyLokalnyyCentr;
        novayaMirovayaPozitsiya.y = ishodnayaPozitsiya.y;
        transform.position = novayaMirovayaPozitsiya;

        Vector3 delta = novayaMirovayaPozitsiya - starayaPozitsiya;
        if (delta.sqrMagnitude > 0.000001f && kontejnerCvetov != null)
        {
            Vector3 localDelta = Quaternion.Inverse(transform.rotation) * delta;
            foreach (Transform child in kontejnerCvetov)
            {
                if (child != null) child.localPosition -= localDelta;
            }
        }

        UdalitCvetyZaGranitsami();

        PostroitMesh();
        ObnovitKollayder();
        ObnovitZabor();
    }

    public void ZakonchitRastyagivanie()
    {
        if (!rastyagivaem) return;
        rastyagivaem = false;
        gran = GranRastyagivaniya.Net;

        Vector3 centr = transform.position;
        float vysotaCentra = PoluchitVisotuTerreina(centr);
        transform.position = new Vector3(centr.x, vysotaCentra, centr.z);
        Postroit();
    }
}