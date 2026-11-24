using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PosadkaDereva : MonoBehaviour
{
    [Header("Откуда идёт луч (если пусто - сам этот объект)")]
    public Transform tochkaStarta;

    [Header("Слой террейна")]
    public LayerMask maskaTerreina;

    [Header("Длина луча")]
    public float dlinaLucha = 30f;

    [Header("Материалы")]
    public Material materialLucha;
    public Material materialPrizraka;

    [Header("Цвета луча")]
    public Color tsvetPopal = Color.green;
    public Color tsvetNePopal = Color.red;

    [Header("Куда складывать объекты ВНЕ секций")]
    public Transform kontejnerDerevev;

    [Header("Работать, пока меню открыто")]
    public bool rabotatPriOtkrytomMenyu = true;

    [Header("Радиусы проверки препятствий")]
    public float radiusProverkiDerevya = 2.5f;
    public float radiusProverkiKusty = 0.5f;
    public float radiusProverkiKlumby = 0.5f;

    [Header("Весь asset с действиями")]
    public InputActionAsset naborDeistvii;
    public string imyaKarty = "XRI Left Interaction";
    public string imyaDeistviya = "PlaceTool";

    private LineRenderer luch;
    private Material materialLuchaKopiya;
    private InputAction deistvieTriggera;

    private GameObject prizrak;
    private GameObject prefabPrizraka;

    private bool luchAktiven = false;
    private bool popalNaTerrein = false;
    private bool mestoSvobodno = false;
    private Vector3 tochkaPopadaniya;

    private UniversalSekciya tekushayaSekciya = null; // Запоминаем секцию

    void Awake()
    {
        luch = GetComponent<LineRenderer>();
        luch.positionCount = 2;
        luch.useWorldSpace = true;
        luch.startWidth = 0.01f;
        luch.endWidth = 0.01f;
        luch.enabled = false;

        if (materialLucha != null) luch.material = materialLucha;
        else { Shader shader = Shader.Find("Universal Render Pipeline/Unlit"); if (shader != null) luch.material = new Material(shader); }
        materialLuchaKopiya = luch.material;

        if (naborDeistvii == null) return;
        var karta = naborDeistvii.FindActionMap(imyaKarty);
        if (karta != null) deistvieTriggera = karta.FindAction(imyaDeistviya);
    }

    void OnEnable()
    {
        if (deistvieTriggera != null)
        {
            deistvieTriggera.performed += NazhatieTriggera;
            deistvieTriggera.Enable();
        }
    }

    void OnDisable()
    {
        if (deistvieTriggera != null) deistvieTriggera.performed -= NazhatieTriggera;
        if (luch != null) luch.enabled = false;
        SpryatPrizrak();
    }

    void LateUpdate()
    {
        GameObject prefab = VyborInstrumenta.TekushiyPrefab;

        bool etoGazon = prefab != null && prefab.GetComponent<GazonTile>() != null;
        Nasazhdenie nas = prefab != null ? prefab.GetComponent<Nasazhdenie>() : null;
        bool etoCvetok = nas != null && nas.kategoriya == "Цветы";
        bool etoSekciya = prefab != null && prefab.GetComponent<UniversalSekciya>() != null;

        bool menyuMeshaet = SostoyanieMenyu.AnyMenuOpen && !rabotatPriOtkrytomMenyu;

        luchAktiven = (prefab != null) && !menyuMeshaet && !etoGazon && !etoCvetok && !etoSekciya;
        popalNaTerrein = false;
        mestoSvobodno = false;
        tekushayaSekciya = null;

        if (!luchAktiven)
        {
            luch.enabled = false;
            SpryatPrizrak();
            return;
        }

        float radius = radiusProverkiKusty;
        string kategoriyaStavim = "";

        if (nas != null)
        {
            kategoriyaStavim = nas.kategoriya;
            if (nas.kategoriya == "Деревья") radius = radiusProverkiDerevya;
            else if (nas.kategoriya == "Клумбы") radius = radiusProverkiKlumby;
        }

        Transform start = tochkaStarta != null ? tochkaStarta : transform;
        Vector3 nachalo = start.position, napravlenie = start.forward;
        Vector3 konets = nachalo + napravlenie * dlinaLucha;

        KlumbaTile tekushayaKlumba = null;

        RaycastHit[] hits = Physics.RaycastAll(nachalo, napravlenie, dlinaLucha, ~0, QueryTriggerInteraction.Ignore);
        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            RaycastHit popadanie = hits[0];

            Nasazhdenie hitNas = popadanie.collider.GetComponentInParent<Nasazhdenie>();

            if (hitNas != null && (hitNas.kategoriya == "Деревья" || hitNas.kategoriya == "Кусты"))
            {
                popalNaTerrein = false;
            }
            else
            {
                tekushayaKlumba = popadanie.collider.GetComponentInParent<KlumbaTile>();
                tekushayaSekciya = popadanie.collider.GetComponentInParent<UniversalSekciya>();
                GazonTile gazon = popadanie.collider.GetComponentInParent<GazonTile>();

                if (tekushayaKlumba != null || tekushayaSekciya != null || gazon != null || ((1 << popadanie.collider.gameObject.layer) & maskaTerreina) != 0)
                {
                    popalNaTerrein = true;
                    tochkaPopadaniya = popadanie.point;
                    konets = popadanie.point;

                    if (tekushayaSekciya != null || gazon != null || tekushayaKlumba != null)
                    {
                        RaycastHit[] groundHits = Physics.RaycastAll(popadanie.point + Vector3.up * 2f, Vector3.down, 10f, ~0, QueryTriggerInteraction.Ignore);
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
                            tochkaPopadaniya = new Vector3(popadanie.point.x, minY, popadanie.point.z);
                            konets = tochkaPopadaniya;
                        }
                    }

                    if (tekushayaKlumba != null && (kategoriyaStavim == "Деревья" || kategoriyaStavim == "Кусты"))
                    {
                        Vector3 localHit = tekushayaKlumba.transform.InverseTransformPoint(tochkaPopadaniya);
                        float otstup = tekushayaKlumba.tolshinaZabora + 0.13f;
                        float maxX = (tekushayaKlumba.shirina / 2f) - otstup;
                        float maxZ = (tekushayaKlumba.dlina / 2f) - otstup;

                        if (Mathf.Abs(localHit.x) > maxX || Mathf.Abs(localHit.z) > maxZ) popalNaTerrein = false;
                    }

                    if (popalNaTerrein)
                    {
                        mestoSvobodno = MestoSvobodno(tochkaPopadaniya, radius, kategoriyaStavim, tekushayaKlumba);

                        // Клумбу нельзя ставить на другую клумбу (или внахлёст на неё)
                        if (mestoSvobodno && kategoriyaStavim == "Клумбы")
                        {
                            if (tekushayaKlumba != null) mestoSvobodno = false;
                            else mestoSvobodno = KlumbaNePeresekaetsya(tochkaPopadaniya, prefab.GetComponent<KlumbaTile>());
                        }

                        if (mestoSvobodno) PokazatPrizrak(prefab, tochkaPopadaniya);
                        else SpryatPrizrak();
                    }
                    else SpryatPrizrak();
                }
                else popalNaTerrein = false;
            }
        }

        if (!popalNaTerrein) SpryatPrizrak();

        luch.enabled = true;
        luch.SetPosition(0, nachalo);
        luch.SetPosition(1, konets);
        UstanovitTsvetLucha(popalNaTerrein && mestoSvobodno ? tsvetPopal : tsvetNePopal);
    }

    private bool MestoSvobodno(Vector3 pozitsiya, float radius, string kategoriyaStavim, KlumbaTile ignoreKlumba)
    {
        Collider[] kollajdery = Physics.OverlapSphere(pozitsiya, radius);
        foreach (var col in kollajdery)
        {
            Nasazhdenie nas = col.GetComponentInParent<Nasazhdenie>();
            if (nas != null)
            {
                if (nas.kategoriya == "Газон") continue;
                if (nas.kategoriya == "Цветы") continue;
                if (nas.kategoriya == "Секции") continue;

                KlumbaTile klumba = nas.GetComponent<KlumbaTile>();
                if (klumba != null && klumba == ignoreKlumba) continue;

                return false;
            }
        }
        return true;
    }

    // Проверяем, что новая клумба не пересекается с уже посаженными клумбами
    private bool KlumbaNePeresekaetsya(Vector3 pozitsiya, KlumbaTile stavimaya)
    {
        float sh = stavimaya != null ? stavimaya.shirina : 1f;
        float dl = stavimaya != null ? stavimaya.dlina : 1f;
        Vector3 half = new Vector3(Mathf.Max(sh, 0.2f) / 2f, 0.5f, Mathf.Max(dl, 0.2f) / 2f);
        Collider[] hits = Physics.OverlapBox(pozitsiya + Vector3.up * 0.5f, half, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (h.GetComponentInParent<KlumbaTile>() != null) return false;
        }
        return true;
    }

    private void NazhatieTriggera(InputAction.CallbackContext context)
    {
        if (!luchAktiven || !popalNaTerrein || !mestoSvobodno) return;

        GameObject prefab = VyborInstrumenta.TekushiyPrefab;
        if (prefab == null) return;

        GameObject novoe = Instantiate(prefab, tochkaPopadaniya, Quaternion.identity, kontejnerDerevev);
        Nasazhdenie nasazhdenie = novoe.GetComponent<Nasazhdenie>();

        if (nasazhdenie != null)
        {
            nasazhdenie.sourcePrefab = prefab;

            // Если навели на секцию и сажаем дерево/куст - делаем их частью секции
            if (tekushayaSekciya != null && (nasazhdenie.kategoriya == "Деревья" || nasazhdenie.kategoriya == "Кусты"))
            {
                tekushayaSekciya.DobavitRastenie(novoe);
            }
        }
    }

    private void UstanovitTsvetLucha(Color tsvet)
    {
        if (materialLuchaKopiya == null) return;
        if (materialLuchaKopiya.HasProperty("_BaseColor")) materialLuchaKopiya.SetColor("_BaseColor", tsvet);
        else if (materialLuchaKopiya.HasProperty("_Color")) materialLuchaKopiya.SetColor("_Color", tsvet);
    }

    private void PokazatPrizrak(GameObject prefab, Vector3 pozitsiya)
    {
        if (prefabPrizraka != prefab) { UnichtozhitPrizrak(); SozdatPrizrak(prefab); }
        if (prizrak != null) { prizrak.SetActive(true); prizrak.transform.position = pozitsiya; }
    }

    private void SpryatPrizrak() { if (prizrak != null) prizrak.SetActive(false); }

    private void SozdatPrizrak(GameObject prefab)
    {
        prefabPrizraka = prefab;
        prizrak = Instantiate(prefab);
        prizrak.name = prefab.name + " (prizrak)";

        Collider[] kollajdery = prizrak.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < kollajdery.Length; i++) kollajdery[i].enabled = false;

        if (materialPrizraka != null)
        {
            Renderer[] otrisovshiki = prizrak.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < otrisovshiki.Length; i++)
            {
                Material[] novyeMaterialy = new Material[otrisovshiki[i].sharedMaterials.Length];
                for (int j = 0; j < novyeMaterialy.Length; j++) novyeMaterialy[j] = materialPrizraka;
                otrisovshiki[i].sharedMaterials = novyeMaterialy;
            }
        }
    }

    private void UnichtozhitPrizrak()
    {
        if (prizrak != null) Destroy(prizrak);
        prizrak = null;
        prefabPrizraka = null;
    }
}