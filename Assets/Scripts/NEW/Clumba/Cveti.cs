using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class Cveti : MonoBehaviour
{
    public Transform tochkaStarta;
    public float dlinaLucha = 30f;
    public Material materialLucha;
    public Material materialPrizraka;
    public Color tsvetPopal = Color.green;
    public Color tsvetNePopal = Color.red;
    public Transform kontejnerCvetovVnezony;

    [Header("Настройки кисти")]
    [Range(0.1f, 3f)] public float diametrKisti = 0.5f;
    public float minimalnoeRasstoyanie = 0.3f;
    public float skorostPosadki = 0.05f;
    public GameObject prefabIndikatoraKisti;
    private GameObject indikatorKisti;

    public InputActionAsset naborDeistvii;
    public string imyaKarty = "XRI Left Interaction";
    public string imyaDeistviya = "PlaceFlower";

    private LineRenderer luch;
    private Material matLuchKopiya;
    private InputAction deistvie;
    private GameObject prizrak;
    private GameObject prefabPrizrak;
    private KlumbaTile tekushayaKlumba;
    private Vector3 tochkaPopadaniya;
    private bool popalNaPoverhnost = false;
    private bool risuem = false;
    private float vremyaSleduyuscheyPosadki = 0f;

    void Awake()
    {
        luch = GetComponent<LineRenderer>();
        luch.positionCount = 2; luch.useWorldSpace = true; luch.startWidth = 0.01f; luch.endWidth = 0.01f;
        luch.enabled = false;
        if (materialLucha != null) luch.material = materialLucha;
        else { Shader s = Shader.Find("Universal Render Pipeline/Unlit"); if (s != null) luch.material = new Material(s); }
        matLuchKopiya = luch.material;
        if (naborDeistvii != null)
        {
            var k = naborDeistvii.FindActionMap(imyaKarty);
            if (k != null) deistvie = k.FindAction(imyaDeistviya);
        }
        if (prefabIndikatoraKisti != null)
        {
            indikatorKisti = Instantiate(prefabIndikatoraKisti);
            indikatorKisti.SetActive(false);
        }
    }

    void OnEnable() { if (deistvie != null) { deistvie.performed += NachatPosadku; deistvie.canceled += ZakonchitPosadku; deistvie.Enable(); } }
    void OnDisable() { if (deistvie != null) { deistvie.performed -= NachatPosadku; deistvie.canceled -= ZakonchitPosadku; } if (luch != null) luch.enabled = false; SpryatatPrizrak(); if (indikatorKisti != null) indikatorKisti.SetActive(false); }

    void LateUpdate()
    {
        GameObject prefab = VyborInstrumenta.TekushiyPrefab;
        if (prefab == null) { luch.enabled = false; SpryatatPrizrak(); indikatorKisti.SetActive(false); return; }
        Nasazhdenie nas = prefab.GetComponent<Nasazhdenie>();
        if (nas == null || nas.kategoriya != "Цветы") { luch.enabled = false; SpryatatPrizrak(); indikatorKisti.SetActive(false); return; }
        if (!luch.enabled) luch.enabled = true;

        Transform start = tochkaStarta != null ? tochkaStarta : transform;
        Vector3 nachalo = start.position, napravlenie = start.forward, konets = nachalo + napravlenie * dlinaLucha;
        popalNaPoverhnost = false;
        tekushayaKlumba = null;

        RaycastHit[] hits = Physics.RaycastAll(nachalo, napravlenie, dlinaLucha, ~0, QueryTriggerInteraction.Ignore);
        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            RaycastHit hit = hits[0];

            KlumbaTile klumba = hit.collider.GetComponentInParent<KlumbaTile>();
            UniversalSekciya sek = hit.collider.GetComponentInParent<UniversalSekciya>();
            GazonTile gazon = hit.collider.GetComponentInParent<GazonTile>();
            Nasazhdenie hitNas = hit.collider.GetComponentInParent<Nasazhdenie>();

            if (hitNas != null && (hitNas.kategoriya == "Деревья" || hitNas.kategoriya == "Кусты"))
            {
                popalNaPoverhnost = false;
            }
            else if (klumba != null)
            {
                Vector3 localHit = klumba.transform.InverseTransformPoint(hit.point);
                float otstup = klumba.tolshinaZabora + 0.02f;
                float maxX = (klumba.shirina / 2f) - otstup;
                float maxZ = (klumba.dlina / 2f) - otstup;

                if (Mathf.Abs(localHit.x) <= maxX && Mathf.Abs(localHit.z) <= maxZ)
                {
                    popalNaPoverhnost = true;
                    tekushayaKlumba = klumba;
                    tochkaPopadaniya = hit.point;
                    konets = hit.point;
                }
            }
            else
            {
                popalNaPoverhnost = true;
                tochkaPopadaniya = hit.point;
                konets = hit.point;
            }

            if (popalNaPoverhnost)
            {
                // Ищем землю (игнорируем все подложки и растения)
                RaycastHit[] groundHits = Physics.RaycastAll(hit.point + Vector3.up * 5f, Vector3.down, 10f, ~0, QueryTriggerInteraction.Ignore);
                float minY = float.MaxValue;
                bool foundGround = false;
                foreach (var gh in groundHits)
                {
                    if (gh.collider.GetComponentInParent<Nasazhdenie>() != null) continue;
                    if (gh.collider.GetComponentInParent<KlumbaTile>() != null) continue;
                    if (gh.collider.GetComponentInParent<UniversalSekciya>() != null) continue;
                    if (gh.collider.GetComponentInParent<GazonTile>() != null) continue;

                    if (gh.point.y < minY) { minY = gh.point.y; foundGround = true; }
                }

                if (foundGround)
                {
                    tochkaPopadaniya = new Vector3(hit.point.x, minY, hit.point.z);
                    konets = tochkaPopadaniya;
                }

                PokazatPrizrak(prefab, tochkaPopadaniya);
                if (indikatorKisti != null)
                {
                    indikatorKisti.SetActive(true);
                    indikatorKisti.transform.position = tochkaPopadaniya + Vector3.up * 0.02f;
                    indikatorKisti.transform.localScale = new Vector3(diametrKisti, 0.01f, diametrKisti);
                }
                if (risuem && Time.time >= vremyaSleduyuscheyPosadki)
                {
                    PosaditKistyu(prefab);
                    vremyaSleduyuscheyPosadki = Time.time + skorostPosadki;
                }
            }
            else { SpryatatPrizrak(); indikatorKisti.SetActive(false); }
        }
        else { SpryatatPrizrak(); indikatorKisti.SetActive(false); }

        luch.SetPosition(0, nachalo);
        luch.SetPosition(1, konets);
        UstanovitTsvetLucha(popalNaPoverhnost ? tsvetPopal : tsvetNePopal);
    }

    private void NachatPosadku(InputAction.CallbackContext ctx)
    {
        risuem = true;
        if (VyborInstrumenta.TekushiyPrefab != null && popalNaPoverhnost)
        {
            Nasazhdenie nas = VyborInstrumenta.TekushiyPrefab.GetComponent<Nasazhdenie>();
            if (nas != null && nas.kategoriya == "Цветы") PosaditOdinochno(VyborInstrumenta.TekushiyPrefab, tochkaPopadaniya);
        }
    }

    private void ZakonchitPosadku(InputAction.CallbackContext ctx) { risuem = false; }

    private void PosaditOdinochno(GameObject prefab, Vector3 pos)
    {
        Transform parent = (tekushayaKlumba != null) ? tekushayaKlumba.kontejnerCvetov : kontejnerCvetovVnezony;
        GameObject novoe = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0, 360), 0), parent);
        Nasazhdenie nas = novoe.GetComponent<Nasazhdenie>();
        if (nas != null) nas.sourcePrefab = prefab;
    }

    private void PosaditKistyu(GameObject prefab)
    {
        int popytki = 0;
        float radius = diametrKisti / 2f;
        while (popytki < 15)
        {
            popytki++;
            Vector2 sluchaynayaTochka = Random.insideUnitCircle * radius;
            Vector3 mirovayaTochka = tochkaPopadaniya + new Vector3(sluchaynayaTochka.x, 0, sluchaynayaTochka.y);

            if (tekushayaKlumba != null)
            {
                Vector3 localPos = tekushayaKlumba.transform.InverseTransformPoint(mirovayaTochka);
                float otstup = tekushayaKlumba.tolshinaZabora + 0.02f;
                if (Mathf.Abs(localPos.x) > (tekushayaKlumba.shirina / 2f) - otstup || Mathf.Abs(localPos.z) > (tekushayaKlumba.dlina / 2f) - otstup) continue;
            }

            RaycastHit[] hits = Physics.RaycastAll(mirovayaTochka + Vector3.up * 5f, Vector3.down, 10f, ~0, QueryTriggerInteraction.Ignore);
            float minY = float.MaxValue;
            bool nashelZemlyu = false;
            foreach (var h in hits)
            {
                if (h.collider.GetComponentInParent<Nasazhdenie>() != null) continue;
                if (h.collider.GetComponentInParent<KlumbaTile>() != null) continue;
                if (h.collider.GetComponentInParent<UniversalSekciya>() != null) continue;
                if (h.collider.GetComponentInParent<GazonTile>() != null) continue;

                if (h.point.y < minY) { minY = h.point.y; nashelZemlyu = true; }
            }

            if (!nashelZemlyu) continue;
            mirovayaTochka = new Vector3(mirovayaTochka.x, minY, mirovayaTochka.z);

            bool mestoSvobodno = true;
            Collider[] overlapColliders = Physics.OverlapSphere(mirovayaTochka, minimalnoeRasstoyanie, ~0, QueryTriggerInteraction.Ignore);
            foreach (var col in overlapColliders)
            {
                Nasazhdenie overlapNas = col.GetComponentInParent<Nasazhdenie>();
                if (overlapNas != null && overlapNas.kategoriya == "Цветы") { mestoSvobodno = false; break; }
            }

            if (mestoSvobodno) { PosaditOdinochno(prefab, mirovayaTochka); break; }
        }
    }

    private void UstanovitTsvetLucha(Color c)
    {
        if (matLuchKopiya == null) return;
        if (matLuchKopiya.HasProperty("_BaseColor")) matLuchKopiya.SetColor("_BaseColor", c);
        else if (matLuchKopiya.HasProperty("_Color")) matLuchKopiya.SetColor("_Color", c);
    }

    private void PokazatPrizrak(GameObject prefab, Vector3 pos)
    {
        if (prefabPrizrak != prefab) { UnichtozhitPrizrak(); SozdatPrizrak(prefab); }
        if (prizrak != null) { prizrak.SetActive(true); prizrak.transform.position = pos; }
    }
    private void SpryatatPrizrak() { if (prizrak != null) prizrak.SetActive(false); }
    private void SozdatPrizrak(GameObject prefab)
    {
        prefabPrizrak = prefab;
        prizrak = Instantiate(prefab);
        Collider[] cols = prizrak.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols) c.enabled = false;
        if (materialPrizraka != null)
        {
            Renderer[] rends = prizrak.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                Material[] mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = materialPrizraka;
                r.sharedMaterials = mats;
            }
        }
    }
    private void UnichtozhitPrizrak() { if (prizrak != null) Destroy(prizrak); prizrak = null; prefabPrizrak = null; }
}