using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class VyborNasazhdeniya : MonoBehaviour
{
    public InventarDeystviy inventar;
    public Transform tochkaStarta;
    public LayerMask maskaPoiska = ~0;
    public LayerMask maskaTerreina;
    public float dlinaLucha = 30f;
    public Material materialLucha;
    public Material materialPodsvetki;
    public Color tsvetNaNasazhdenii = Color.red;
    public Color tsvetPustoi = Color.white;
    public InputActionAsset naborDeistvii;
    public string imyaKarty = "XRI Right Interaction";
    public string imyaDeistviya = "PickTool";
    public string imyaKartyGrip = "XRI Right Interaction";
    public string imyaDeistviyaGrip = "MoveTool";

    [Header("=== Ластик для цветов ===")]
    public Cveti skriptPosadkiCvetov;
    public GameObject prefabIndikatoraLastika;
    private GameObject indikatorLastika;
    private bool rezhimLastika = false;
    private float vremyaSleduyuschegoUdaleniya = 0f;

    private LineRenderer luch;
    private Material materialLuchaKopiya;
    private InputAction deistvieTriggera, deistvieGrip;

    private Nasazhdenie navedennoe, navedennayaChast, vybrannoe, peremeshchaemyi;
    private GazonTile rastyagivaemyiGazon;
    private UniversalSekciya rastyagivaemayaSekciya;
    private KlumbaTile rastyagivaemayaKlumba;

    private Vector3 poslednyayaTochka, poslednyayaNapravlenie, poslednyayaNachalo;
    private bool estTochka = false;

    void Awake()
    {
        luch = GetComponent<LineRenderer>();
        luch.positionCount = 2; luch.useWorldSpace = true; luch.startWidth = 0.01f; luch.endWidth = 0.01f;
        luch.enabled = false;
        if (materialLucha != null) luch.material = materialLucha;
        else { Shader s = Shader.Find("Universal Render Pipeline/Unlit"); if (s != null) luch.material = new Material(s); }
        materialLuchaKopiya = luch.material;

        if (naborDeistvii == null) return;
        var k = naborDeistvii.FindActionMap(imyaKarty); if (k != null) deistvieTriggera = k.FindAction(imyaDeistviya);
        var kg = naborDeistvii.FindActionMap(imyaKartyGrip); if (kg != null) deistvieGrip = kg.FindAction(imyaDeistviyaGrip);

        if (prefabIndikatoraLastika != null)
        {
            indikatorLastika = Instantiate(prefabIndikatoraLastika);
            indikatorLastika.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (deistvieTriggera != null) { deistvieTriggera.performed += NazhatieTriggera; deistvieTriggera.Enable(); }
        if (deistvieGrip != null) { deistvieGrip.performed += NazhatieGrip; deistvieGrip.canceled += OtpuskanieGrip; deistvieGrip.Enable(); }
    }

    void OnDisable()
    {
        if (deistvieTriggera != null) deistvieTriggera.performed -= NazhatieTriggera;
        if (deistvieGrip != null) { deistvieGrip.performed -= NazhatieGrip; deistvieGrip.canceled -= OtpuskanieGrip; }
        luch.enabled = false;
        peremeshchaemyi = null; rastyagivaemyiGazon = null; rastyagivaemayaSekciya = null; rastyagivaemayaKlumba = null;
        VyklyuchitRezhimLastika();
    }

    public void VklyuchitRezhimLastika()
    {
        rezhimLastika = true;
        if (indikatorLastika != null) indikatorLastika.SetActive(true);
    }

    public void VyklyuchitRezhimLastika()
    {
        rezhimLastika = false;
        if (indikatorLastika != null) indikatorLastika.SetActive(false);
    }

    void LateUpdate()
    {
        if (!SostoyanieMenyu.InventoryOpen) { luch.enabled = false; SbrositVydelenie(); return; }

        Transform start = tochkaStarta != null ? tochkaStarta : transform;
        Vector3 nachalo = start.position, napravlenie = start.forward, konets = nachalo + napravlenie * dlinaLucha;

        if (rezhimLastika)
        {
            if (Physics.Raycast(nachalo, napravlenie, out RaycastHit hitLastika, dlinaLucha, ~0, QueryTriggerInteraction.Ignore))
            {
                konets = hitLastika.point;
                float radius = skriptPosadkiCvetov != null ? skriptPosadkiCvetov.diametrKisti : 0.5f;
                if (indikatorLastika != null)
                {
                    indikatorLastika.transform.position = hitLastika.point + Vector3.up * 0.02f;
                    indikatorLastika.transform.localScale = new Vector3(radius, 0.01f, radius);
                }
            }
            luch.enabled = true; luch.SetPosition(0, nachalo); luch.SetPosition(1, konets);
            UstanovitTsvetLucha(Color.magenta);
            return;
        }

        if (rastyagivaemyiGazon != null)
        {
            rastyagivaemyiGazon.ObnovitRastyagivanie(nachalo, napravlenie);
            konets = rastyagivaemyiGazon.ProektirovatLuchNaPloskost(nachalo, napravlenie);
            luch.enabled = true; luch.SetPosition(0, nachalo); luch.SetPosition(1, konets);
            UstanovitTsvetLucha(Color.yellow); return;
        }

        if (rastyagivaemayaSekciya != null)
        {
            rastyagivaemayaSekciya.ObnovitRastyagivanie(nachalo, napravlenie);
            konets = rastyagivaemayaSekciya.ProektirovatLuchNaPloskost(nachalo, napravlenie);
            luch.enabled = true; luch.SetPosition(0, nachalo); luch.SetPosition(1, konets);
            UstanovitTsvetLucha(Color.yellow); return;
        }

        if (rastyagivaemayaKlumba != null)
        {
            rastyagivaemayaKlumba.ObnovitRastyagivanie(nachalo, napravlenie);
            konets = rastyagivaemayaKlumba.ProektirovatLuchNaPloskost(nachalo, napravlenie);
            luch.enabled = true; luch.SetPosition(0, nachalo); luch.SetPosition(1, konets);
            UstanovitTsvetLucha(Color.yellow); return;
        }

        if (peremeshchaemyi != null)
        {
            if (Physics.Raycast(nachalo, napravlenie, out RaycastHit hitT, dlinaLucha, maskaTerreina, QueryTriggerInteraction.Ignore))
            {
                Vector3 targetPos = hitT.point;

                if (peremeshchaemyi.kategoriya == "Цветы")
                {
                    KlumbaTile rodKlumba = peremeshchaemyi.GetComponentInParent<KlumbaTile>();
                    if (rodKlumba != null)
                    {
                        Vector3 localPos = rodKlumba.transform.InverseTransformPoint(targetPos);
                        float otstup = rodKlumba.tolshinaZabora + 0.02f;
                        float maxX = (rodKlumba.shirina / 2f) - otstup;
                        float maxZ = (rodKlumba.dlina / 2f) - otstup;

                        localPos.x = Mathf.Clamp(localPos.x, -maxX, maxX);
                        localPos.z = Mathf.Clamp(localPos.z, -maxZ, maxZ);

                        targetPos = rodKlumba.transform.TransformPoint(localPos);

                        if (Physics.Raycast(targetPos + Vector3.up * 5f, Vector3.down, out RaycastHit groundHit, 10f, maskaTerreina))
                        {
                            targetPos.y = groundHit.point.y;
                        }
                    }
                }
                else if (peremeshchaemyi.kategoriya == "Секции")
                {
                    UniversalSekciya sek = peremeshchaemyi.GetComponentInParent<UniversalSekciya>();
                    float shirinaSek = 1f;
                    float dlinaSek = 1f;
                    if (sek != null)
                    {
                        shirinaSek = sek.shirina;
                        dlinaSek = sek.dlina;
                    }

                    Vector3 halfExtents = new Vector3(shirinaSek / 2f, 1f, dlinaSek / 2f);
                    Collider[] cols = Physics.OverlapBox(targetPos + Vector3.up * 0.5f, halfExtents, peremeshchaemyi.transform.rotation, ~0, QueryTriggerInteraction.Ignore);

                    bool zablokirovano = false;
                    foreach (var c in cols)
                    {
                        if (c.transform.IsChildOf(peremeshchaemyi.transform) || c.transform == peremeshchaemyi.transform) continue;

                        if (c.GetComponentInParent<KlumbaTile>() != null) { zablokirovano = true; break; }
                    }

                    if (zablokirovano) targetPos = peremeshchaemyi.transform.position;
                }

                konets = targetPos;
                peremeshchaemyi.transform.position = targetPos;

                if (peremeshchaemyi.TryGetComponent<GazonTile>(out var gt)) gt.PerestroiMeshIKollayder();
            }

            luch.enabled = true; luch.SetPosition(0, nachalo); luch.SetPosition(1, konets);
            UstanovitTsvetLucha(tsvetNaNasazhdenii); return;
        }

        Nasazhdenie obshiy = null, chast = null;
        if (Physics.Raycast(nachalo, napravlenie, out RaycastHit hit, dlinaLucha, maskaPoiska, QueryTriggerInteraction.Ignore))
        {
            konets = hit.point; poslednyayaTochka = hit.point; poslednyayaNapravlenie = napravlenie; poslednyayaNachalo = nachalo; estTochka = true;

            Nasazhdenie nas = hit.collider.GetComponentInParent<Nasazhdenie>();
            if (nas != null)
            {
                UniversalSekciya sek = nas.GetComponentInParent<UniversalSekciya>();
                if (sek != null)
                {
                    obshiy = sek.GetComponent<Nasazhdenie>();
                    if (nas != obshiy) chast = nas;
                }
                else obshiy = nas;
            }
        }
        else { estTochka = false; poslednyayaNapravlenie = napravlenie; poslednyayaNachalo = nachalo; }

        ObnovitNavedenie(obshiy, chast);
        luch.enabled = true; luch.SetPosition(0, nachalo); luch.SetPosition(1, konets);
        UstanovitTsvetLucha(obshiy != null ? tsvetNaNasazhdenii : tsvetPustoi);
    }

    public void SbrositVydelenie()
    {
        SnyatPodsvetku(navedennoe); SnyatPodsvetku(navedennayaChast); SnyatPodsvetku(vybrannoe);

        if (rastyagivaemyiGazon != null) { rastyagivaemyiGazon.ZakonchitRastyagivanie(); rastyagivaemyiGazon = null; }
        if (rastyagivaemayaSekciya != null) { rastyagivaemayaSekciya.ZakonchitRastyagivanie(); rastyagivaemayaSekciya = null; }
        if (rastyagivaemayaKlumba != null) { rastyagivaemayaKlumba.ZakonchitRastyagivanie(); rastyagivaemayaKlumba = null; }

        navedennoe = null; navedennayaChast = null; vybrannoe = null; peremeshchaemyi = null;
        VyklyuchitRezhimLastika();
        if (inventar != null && inventar.tekushiyObyekt != null) inventar.UstanovitTekushiyObyekt(null);
    }

    private void ObnovitNavedenie(Nasazhdenie novoe, Nasazhdenie chast)
    {
        if (novoe == navedennoe && chast == navedennayaChast) return;
        if (navedennoe != null && navedennoe != vybrannoe) SnyatPodsvetku(navedennoe);
        navedennoe = novoe; navedennayaChast = chast;
        if (vybrannoe == null || vybrannoe != navedennoe)
            if (navedennoe != null && (vybrannoe == null || !vybrannoe.transform.IsChildOf(navedennoe.transform)))
                Podsvetit(navedennoe);
    }

    private void NazhatieTriggera(InputAction.CallbackContext ctx)
    {
        if (!SostoyanieMenyu.InventoryOpen) return;

        if (rezhimLastika)
        {
            if (Time.time >= vremyaSleduyuschegoUdaleniya && Physics.Raycast(tochkaStarta != null ? tochkaStarta.position : transform.position, transform.forward, out RaycastHit hitLast, dlinaLucha, ~0, QueryTriggerInteraction.Ignore))
            {
                float radius = skriptPosadkiCvetov != null ? (skriptPosadkiCvetov.diametrKisti / 2f) : 0.25f;
                Collider[] cols = Physics.OverlapSphere(hitLast.point, radius);

                foreach (var col in cols)
                {
                    Nasazhdenie nas = col.GetComponentInParent<Nasazhdenie>();
                    if (nas != null && nas.kategoriya == "Цветы")
                    {
                        Destroy(nas.gameObject);
                        if (inventar != null && inventar.tekushiyObyekt == nas)
                        {
                            inventar.UstanovitTekushiyObyekt(null);
                        }
                    }
                }
                vremyaSleduyuschegoUdaleniya = Time.time + 0.05f;
            }
            return;
        }

        if (navedennoe == null) return;
        Nasazhdenie vybratChto = navedennoe;
        if (navedennayaChast != null && inventar != null && inventar.tekushiyObyekt == navedennoe) vybratChto = navedennayaChast;

        SnyatPodsvetku(vybrannoe); SnyatPodsvetku(navedennoe);
        vybrannoe = vybratChto;
        if (inventar != null) inventar.UstanovitTekushiyObyekt(vybrannoe);
        Podsvetit(vybrannoe);
    }

    private void NazhatieGrip(InputAction.CallbackContext ctx)
    {
        if (!SostoyanieMenyu.InventoryOpen || navedennoe == null || rezhimLastika) return;

        if (inventar != null && inventar.RezhimRastyagivaniya && estTochka)
        {
            if (navedennoe.TryGetComponent<GazonTile>(out var gt) && gt.NachatRastyagivanie(poslednyayaTochka)) { rastyagivaemyiGazon = gt; return; }

            // Ищем компонент секции
            UniversalSekciya sn = navedennoe.GetComponentInParent<UniversalSekciya>();
            if (sn != null && sn.NachatRastyagivanie(poslednyayaTochka)) { rastyagivaemayaSekciya = sn; return; }

            if (navedennoe.TryGetComponent<KlumbaTile>(out var kt) && kt.NachatRastyagivanie(poslednyayaTochka)) { rastyagivaemayaKlumba = kt; return; }
        }
        peremeshchaemyi = navedennoe;
    }

    private void OtpuskanieGrip(InputAction.CallbackContext ctx)
    {
        if (rastyagivaemyiGazon != null) { rastyagivaemyiGazon.ZakonchitRastyagivanie(); rastyagivaemyiGazon = null; }
        if (rastyagivaemayaSekciya != null) { rastyagivaemayaSekciya.ZakonchitRastyagivanie(); rastyagivaemayaSekciya = null; }
        if (rastyagivaemayaKlumba != null) { rastyagivaemayaKlumba.ZakonchitRastyagivanie(); rastyagivaemayaKlumba = null; }
        if (peremeshchaemyi != null) { if (peremeshchaemyi.TryGetComponent<GazonTile>(out var gt)) gt.Postroit(); peremeshchaemyi = null; }
    }

    private void Podsvetit(Nasazhdenie n) { if (n == null) return; var p = n.GetComponentInParent<PodsvetkaNasazhdeniya>(); if (p == null) p = n.gameObject.AddComponent<PodsvetkaNasazhdeniya>(); p.Vkluchit(materialPodsvetki); }
    private void SnyatPodsvetku(Nasazhdenie n) { if (n == null) return; var p = n.GetComponentInParent<PodsvetkaNasazhdeniya>(); if (p != null) p.Vyklyuchit(); }
    private void UstanovitTsvetLucha(Color c) { if (materialLuchaKopiya == null) return; if (materialLuchaKopiya.HasProperty("_BaseColor")) materialLuchaKopiya.SetColor("_BaseColor", c); else if (materialLuchaKopiya.HasProperty("_Color")) materialLuchaKopiya.SetColor("_Color", c); }
}