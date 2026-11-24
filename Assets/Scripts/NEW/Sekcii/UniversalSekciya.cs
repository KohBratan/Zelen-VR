using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class UniversalSekciya : MonoBehaviour
{
    public enum GranRastyagivaniya { Net, Pered, Zad, Levo, Pravo }

    [Header("Размеры секции")]
    public float shirina = 20f;
    public float dlina = 20f;
    public float minRazmer = 5f;
    public float maxRazmer = 100f;

    [Header("Минимальное расстояние между растениями")]
    public float shagDerevya = 3f;
    public float shagKusty = 1f;

    [Header("Слой террейна")]
    public LayerMask maskaTerreina;

    public bool etoPrizrak = false;

    private Mesh mesh;
    private BoxCollider boxCollider;

    private List<GameObject> derevya = new List<GameObject>();
    private List<GameObject> kusty = new List<GameObject>();

    private bool rastyagivaem = false;
    private GranRastyagivaniya gran = GranRastyagivaniya.Net;
    private Vector3 ishodnayaPozitsiya;
    private Quaternion ishodnyyRotation;
    private float yakorLokalnyyX;
    private float yakorLokalnyyZ;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "SekciyaMesh";
        GetComponent<MeshFilter>().mesh = mesh;
        boxCollider = GetComponent<BoxCollider>();
    }

    void Start()
    {
        PostroitPodlozhku();
        ObnovitKollayder();
        if (!etoPrizrak) ZapolnitPustotu();
    }

    // НОВЫЙ МЕТОД: Добавление растения, посаженного вручную
    public void DobavitRastenie(GameObject obj)
    {
        Nasazhdenie nas = obj.GetComponent<Nasazhdenie>();
        if (nas != null)
        {
            if (nas.kategoriya == "Деревья") derevya.Add(obj);
            else if (nas.kategoriya == "Кусты") kusty.Add(obj);

            obj.transform.SetParent(transform);
        }
    }

    private float PoluchitVisotuTerreina(Vector3 mirovaya)
    {
        if (Physics.Raycast(mirovaya + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f, maskaTerreina))
            return hit.point.y;
        return mirovaya.y;
    }

    private void PostroitPodlozhku()
    {
        mesh.Clear();
        int subdiv = Mathf.Max(1, Mathf.FloorToInt(shirina / 2f));
        int subdivZ = Mathf.Max(1, Mathf.FloorToInt(dlina / 2f));

        int vershinX = subdiv + 1;
        int vershinZ = subdivZ + 1;
        Vector3[] verts = new Vector3[vershinX * vershinZ];

        for (int iz = 0; iz < vershinZ; iz++)
        {
            for (int ix = 0; ix < vershinX; ix++)
            {
                float tx = (float)ix / subdiv;
                float tz = (float)iz / subdivZ;
                float lx = Mathf.Lerp(-shirina / 2f, shirina / 2f, tx);
                float lz = Mathf.Lerp(-dlina / 2f, dlina / 2f, tz);
                Vector3 mestnaya = new Vector3(lx, 0, lz);
                Vector3 mirovaya = transform.TransformPoint(mestnaya);
                mirovaya.y = PoluchitVisotuTerreina(mirovaya);
                verts[iz * vershinX + ix] = transform.InverseTransformPoint(mirovaya);
            }
        }

        int[] triangles = new int[subdiv * subdivZ * 6];
        int idx = 0;
        for (int iz = 0; iz < subdivZ; iz++)
        {
            for (int ix = 0; ix < subdiv; ix++)
            {
                int v0 = iz * vershinX + ix;
                int v1 = v0 + 1;
                int v2 = v0 + vershinX;
                int v3 = v2 + 1;
                triangles[idx++] = v0; triangles[idx++] = v2; triangles[idx++] = v1;
                triangles[idx++] = v1; triangles[idx++] = v2; triangles[idx++] = v3;
            }
        }
        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void ObnovitKollayder()
    {
        if (boxCollider == null) return;
        Bounds b = mesh.bounds;
        boxCollider.center = new Vector3(b.center.x, b.center.y + 0.05f, b.center.z);
        boxCollider.size = new Vector3(Mathf.Max(b.size.x, 0.1f), Mathf.Max(b.size.y + 0.1f, 0.1f), Mathf.Max(b.size.z, 0.1f));
    }

    private void ZapolnitPustotu()
    {
        float area = shirina * dlina;
        int maxTreesSlots = Mathf.FloorToInt(area / (shagDerevya * shagDerevya));
        int maxBushesSlots = Mathf.FloorToInt(area / (shagKusty * shagKusty));

        int targetTrees = Mathf.RoundToInt(maxTreesSlots * VyborSekcii.Plotnost);
        int targetBushes = Mathf.RoundToInt(maxBushesSlots * VyborSekcii.Plotnost);

        if (VyborSekcii.Derevya.Count > 0)
        {
            int treesToAdd = targetTrees - derevya.Count;
            int attempts = 0;
            while (treesToAdd > 0 && attempts < maxTreesSlots * 30)
            {
                attempts++;
                float lx = Random.Range(-shirina / 2f, shirina / 2f);
                float lz = Random.Range(-dlina / 2f, dlina / 2f);
                Vector3 pos = transform.TransformPoint(new Vector3(lx, 0, lz));
                pos.y = PoluchitVisotuTerreina(pos);

                bool tooClose = false;
                foreach (var d in derevya)
                {
                    if (d != null && Vector3.Distance(d.transform.position, pos) < shagDerevya) { tooClose = true; break; }
                }

                if (!tooClose)
                {
                    GameObject prefab = VyborSekcii.Derevya[Random.Range(0, VyborSekcii.Derevya.Count)];
                    if (prefab != null)
                    {
                        GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), transform);
                        derevya.Add(obj);
                        treesToAdd--;
                    }
                }
            }
        }

        if (VyborSekcii.Kusty.Count > 0)
        {
            int bushesToAdd = targetBushes - kusty.Count;
            int attempts = 0;
            while (bushesToAdd > 0 && attempts < maxBushesSlots * 30)
            {
                attempts++;
                float lx = Random.Range(-shirina / 2f, shirina / 2f);
                float lz = Random.Range(-dlina / 2f, dlina / 2f);
                Vector3 pos = transform.TransformPoint(new Vector3(lx, 0, lz));
                pos.y = PoluchitVisotuTerreina(pos);

                bool tooClose = false;
                foreach (var b in kusty)
                {
                    if (b != null && Vector3.Distance(b.transform.position, pos) < shagKusty) { tooClose = true; break; }
                }

                if (!tooClose)
                {
                    foreach (var d in derevya)
                    {
                        if (d != null && Vector3.Distance(d.transform.position, pos) < 1.0f) { tooClose = true; break; }
                    }
                }

                if (!tooClose)
                {
                    GameObject prefab = VyborSekcii.Kusty[Random.Range(0, VyborSekcii.Kusty.Count)];
                    if (prefab != null)
                    {
                        GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), transform);
                        kusty.Add(obj);
                        bushesToAdd--;
                    }
                }
            }
        }
    }

    private void UdalitZaGranitsami()
    {
        float halfX = shirina / 2f;
        float halfZ = dlina / 2f;

        for (int i = derevya.Count - 1; i >= 0; i--)
        {
            if (derevya[i] == null) { derevya.RemoveAt(i); continue; }
            Vector3 lp = derevya[i].transform.localPosition;
            if (Mathf.Abs(lp.x) > halfX || Mathf.Abs(lp.z) > halfZ) { Destroy(derevya[i]); derevya.RemoveAt(i); }
        }

        for (int i = kusty.Count - 1; i >= 0; i--)
        {
            if (kusty[i] == null) { kusty.RemoveAt(i); continue; }
            Vector3 lp = kusty[i].transform.localPosition;
            if (Mathf.Abs(lp.x) > halfX || Mathf.Abs(lp.z) > halfZ) { Destroy(kusty[i]); kusty.RemoveAt(i); }
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

        float tmpShirina = shirina;
        float tmpDlina = dlina;

        switch (gran)
        {
            case GranRastyagivaniya.Pravo:
                tmpShirina = Mathf.Clamp(lokalnaya.x - yakorLokalnyyX, minRazmer, maxRazmer);
                novyyLokalnyyCentr = new Vector3(yakorLokalnyyX + tmpShirina / 2f, 0f, 0f);
                break;
            case GranRastyagivaniya.Levo:
                tmpShirina = Mathf.Clamp(yakorLokalnyyX - lokalnaya.x, minRazmer, maxRazmer);
                novyyLokalnyyCentr = new Vector3(yakorLokalnyyX - tmpShirina / 2f, 0f, 0f);
                break;
            case GranRastyagivaniya.Pered:
                tmpDlina = Mathf.Clamp(yakorLokalnyyZ - lokalnaya.z, minRazmer, maxRazmer);
                novyyLokalnyyCentr = new Vector3(0f, 0f, yakorLokalnyyZ - tmpDlina / 2f);
                break;
            case GranRastyagivaniya.Zad:
                tmpDlina = Mathf.Clamp(lokalnaya.z - yakorLokalnyyZ, minRazmer, maxRazmer);
                novyyLokalnyyCentr = new Vector3(0f, 0f, yakorLokalnyyZ + tmpDlina / 2f);
                break;
        }

        Vector3 novayaMirovayaPozitsiya = ishodnayaPozitsiya + ishodnyyRotation * novyyLokalnyyCentr;
        novayaMirovayaPozitsiya.y = PoluchitVisotuTerreina(novayaMirovayaPozitsiya);

        // ПРОВЕРКА НА ПУСТОТУ И КОЛЛИЗИИ
        if (tmpShirina > shirina || tmpDlina > dlina)
        {
            Vector3 halfSize = new Vector3(tmpShirina / 2f - 0.5f, 1f, tmpDlina / 2f - 0.5f);
            Vector3[] corners = {
                novayaMirovayaPozitsiya + ishodnyyRotation * new Vector3(halfSize.x, 0, halfSize.z),
                novayaMirovayaPozitsiya + ishodnyyRotation * new Vector3(-halfSize.x, 0, halfSize.z),
                novayaMirovayaPozitsiya + ishodnyyRotation * new Vector3(halfSize.x, 0, -halfSize.z),
                novayaMirovayaPozitsiya + ishodnyyRotation * new Vector3(-halfSize.x, 0, -halfSize.z)
            };
            foreach (var corner in corners)
            {
                if (!Physics.Raycast(corner + Vector3.up * 10f, Vector3.down, out _, 20f, maskaTerreina)) return;
            }

            Collider[] hits = Physics.OverlapBox(novayaMirovayaPozitsiya + Vector3.up * 1f, new Vector3(tmpShirina / 2f - 0.5f, 1f, tmpDlina / 2f - 0.5f), ishodnyyRotation, ~0, QueryTriggerInteraction.Ignore);
            foreach (var h in hits)
            {
                if (h.transform.IsChildOf(transform) || h.transform == transform) continue;
                if (h.GetComponentInParent<UniversalSekciya>() != null || h.GetComponentInParent<KlumbaTile>() != null) return;
            }
        }

        shirina = tmpShirina;
        dlina = tmpDlina;
        transform.position = novayaMirovayaPozitsiya;

        Vector3 delta = novayaMirovayaPozitsiya - starayaPozitsiya;
        if (delta.sqrMagnitude > 0.000001f)
        {
            Vector3 localDelta = Quaternion.Inverse(transform.rotation) * delta;
            foreach (var d in derevya) if (d != null) d.transform.localPosition -= localDelta;
            foreach (var k in kusty) if (k != null) k.transform.localPosition -= localDelta;
        }

        PostroitPodlozhku();
        ObnovitKollayder();
        UdalitZaGranitsami();
        ZapolnitPustotu();
    }

    public void ZakonchitRastyagivanie()
    {
        if (!rastyagivaem) return;
        rastyagivaem = false;
        gran = GranRastyagivaniya.Net;

        Vector3 centr = transform.position;
        float vysotaCentra = PoluchitVisotuTerreina(centr);
        transform.position = new Vector3(centr.x, vysotaCentra, centr.z);

        PostroitPodlozhku();
        ObnovitKollayder();
    }
}