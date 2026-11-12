using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PosadkaDereva : MonoBehaviour
{
    [Header("Откуда идёт луч (если пусто - сам этот объект)")]
    public Transform tochkaStarta;

    [Header("Слой террейна, по которому стреляет луч")]
    public LayerMask maskaTerreina;

    [Header("Длина луча")]
    public float dlinaLucha = 30f;

    [Header("Материал луча и материал призрака (прозрачный)")]
    public Material materialLucha;
    public Material materialPrizraka;

    [Header("Цвет луча: попал / не попал или занято")]
    public Color tsvetPopal = Color.green;
    public Color tsvetNePopal = Color.red;

    [Header("Куда складывать посаженные объекты")]
    public Transform kontejnerDerevev;

    [Header("Работать, пока меню открыто")]
    public bool rabotatPriOtkrytomMenyu = true;

    [Header("Радиусы проверки препятствий")]
    public float radiusProverkiDerevya = 2.5f;
    public float radiusProverkiKusty = 0.5f;
    public float radiusProverkiSekcii = 5f;
    public float radiusProverkiKlumby = 0.5f;

    [Header("Весь asset с действиями")]
    public InputActionAsset naborDeistvii;

    [Header("Карта и имя действия триггера")]
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

    void OnDestroy() { UnichtozhitPrizrak(); }

    void LateUpdate()
    {
        GameObject prefab = VyborInstrumenta.TekushiyPrefab;
        
        bool etoGazon = prefab != null && prefab.GetComponent<GazonTile>() != null;
        Nasazhdenie nas = prefab != null ? prefab.GetComponent<Nasazhdenie>() : null;
        bool etoCvetok = nas != null && nas.kategoriya == "Цветы"; 

        bool menyuMeshaet = SostoyanieMenyu.AnyMenuOpen && !rabotatPriOtkrytomMenyu;

        luchAktiven = (prefab != null) && !menyuMeshaet && !etoGazon && !etoCvetok;
        popalNaTerrein = false;
        mestoSvobodno = false;

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
            else if (nas.kategoriya == "Секции") radius = radiusProverkiSekcii;
            else if (nas.kategoriya == "Клумбы") radius = radiusProverkiKlumby;
        }

        Transform start = tochkaStarta != null ? tochkaStarta : transform;
        Vector3 nachalo = start.position, napravlenie = start.forward;
        Vector3 konets = nachalo + napravlenie * dlinaLucha;

        if (Physics.Raycast(nachalo, napravlenie, out RaycastHit popadanie, dlinaLucha, maskaTerreina, QueryTriggerInteraction.Ignore))
        {
            popalNaTerrein = true;
            tochkaPopadaniya = popadanie.point;
            konets = popadanie.point;

            mestoSvobodno = MestoSvobodno(tochkaPopadaniya, radius, kategoriyaStavim);
            
            // НОВАЯ ПРОВЕРКА: Если пытаемся поставить что-то в клумбу - запрещаем
            if (mestoSvobodno && EstLiKlumba(tochkaPopadaniya))
            {
                mestoSvobodno = false; // Внутрь клумбы нельзя!
            }

            if (mestoSvobodno) PokazatPrizrak(prefab, popadanie.point);
            else SpryatPrizrak();
        }
        else SpryatPrizrak();

        luch.enabled = true;
        luch.SetPosition(0, nachalo);
        luch.SetPosition(1, konets);
        UstanovitTsvetLucha(popalNaTerrein && mestoSvobodno ? tsvetPopal : tsvetNePopal);
    }

    // Проверка, есть ли в этой точке клумба
    private bool EstLiKlumba(Vector3 tochka)
    {
        // Сканируем сферу чуть выше земли, чтобы поймать коллайдер клумбы
        Collider[] kollajdery = Physics.OverlapSphere(tochka + Vector3.up * 0.2f, 0.2f);
        foreach (var col in kollajdery)
        {
            if (col.GetComponentInParent<KlumbaTile>() != null) return true;
        }
        return false;
    }

    private bool MestoSvobodno(Vector3 pozitsiya, float radius, string kategoriyaStavim)
    {
        Collider[] kollajdery = Physics.OverlapSphere(pozitsiya, radius);
        foreach (var col in kollajdery)
        {
            Nasazhdenie nas = col.GetComponentInParent<Nasazhdenie>();
            if (nas != null)
            {
                if (nas.kategoriya == "Газон")
                {
                    if (kategoriyaStavim == "Клумбы") return false;
                    continue; 
                }
                if (nas.kategoriya == "Цветы") continue;
                return false;
            }
        }
        return true;
    }

    private void NazhatieTriggera(InputAction.CallbackContext context)
    {
        if (!luchAktiven || !popalNaTerrein || !mestoSvobodno) return;

        GameObject prefab = VyborInstrumenta.TekushiyPrefab;
        if (prefab == null || prefab.GetComponent<GazonTile>() != null) return;
        
        Nasazhdenie nasPrefab = prefab.GetComponent<Nasazhdenie>();
        if (nasPrefab != null && nasPrefab.kategoriya == "Цветы") return;

        GameObject novoe = Instantiate(prefab, tochkaPopadaniya, Quaternion.identity, kontejnerDerevev);

        Nasazhdenie nasazhdenie = novoe.GetComponent<Nasazhdenie>();
        if (nasazhdenie != null) nasazhdenie.sourcePrefab = prefab;

        Debug.Log("Посажено: " + novoe.name);
    }

    private void UstanovitTsvetLucha(Color tsvet)
    {
        if (materialLuchaKopiya == null) return;
        if (materialLuchaKopiya.HasProperty("_BaseColor")) materialLuchaKopiya.SetColor("_BaseColor", tsvet);
        else if (materialLuchaKopiya.HasProperty("_Color")) materialLuchaKopiya.SetColor("_Color", tsvet);
    }

    private void PokazatPrizrak(GameObject prefab, Vector3 pozitsiya)
    {
        if (prefabPrizraka != prefab)
        {
            UnichtozhitPrizrak();
            SozdatPrizrak(prefab);
        }
        if (prizrak != null)
        {
            prizrak.SetActive(true);
            prizrak.transform.position = pozitsiya;
        }
    }

    private void SpryatPrizrak() { if (prizrak != null) prizrak.SetActive(false); }

    private void SozdatPrizrak(GameObject prefab)
    {
        prefabPrizraka = prefab;
        prizrak = Instantiate(prefab);
        prizrak.name = prefab.name + " (prizrak)";

        if (prizrak.TryGetComponent<SekciyaNasazhdeniy>(out var sek)) sek.etoPrizrak = true;

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
