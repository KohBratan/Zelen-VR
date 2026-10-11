using UnityEngine;
using UnityEngine.InputSystem;

// Вешается на пустой объект внутри Left Controller (LineRenderer добавится сам).
// Когда в меню выбрано дерево: рисует луч от левой руки, и если луч попал
// на террейн - показывает там прозрачный "призрак" дерева.
// Левый триггер - посадить дерево в этой точке.
[RequireComponent(typeof(LineRenderer))]
public class PosadkaDereva : MonoBehaviour
{
    [Header("Откуда идёт луч (если пусто - сам этот объект)")]
    public Transform tochkaStarta;

    [Header("Слой террейна, по которому стреляет луч")]
    public LayerMask maskaTerreina;

    [Header("Длина луча")]
    public float dlinaLucha = 30f;

    [Header("Материал луча (необязательно, лучше URP/Unlit) и материал призрака (прозрачный)")]
    public Material materialLucha;
    public Material materialPrizraka;

    [Header("Цвет луча: попал на террейн / не попал")]
    public Color tsvetPopal = Color.green;
    public Color tsvetNePopal = Color.red;

    [Header("Куда складывать посаженные деревья (необязательно)")]
    public Transform kontejnerDerevev;

    [Header("Работать, пока меню открыто (нужно, если выбор сбрасывается при закрытии меню)")]
    public bool rabotatPriOtkrytomMenyu = true;

    [Header("Весь asset с действиями")]
    public InputActionAsset naborDeistvii;

    [Header("Карта и имя действия триггера (Button)")]
    public string imyaKarty = "XRI Left Interaction";
    public string imyaDeistviya = "PlaceTool";

    private LineRenderer luch;
    private Material materialLuchaKopiya;
    private InputAction deistvieTriggera;

    private GameObject prizrak;
    private GameObject prefabPrizraka;

    private bool luchAktiven = false;
    private bool popalNaTerrein = false;
    private Vector3 tochkaPopadaniya;

    void Awake()
    {
        luch = GetComponent<LineRenderer>();
        luch.positionCount = 2;
        luch.useWorldSpace = true;
        luch.startWidth = 0.01f;
        luch.endWidth = 0.01f;
        luch.startColor = Color.white;
        luch.endColor = Color.white;
        luch.enabled = false;

        if (materialLucha != null)
        {
            luch.material = materialLucha;
        }
        else
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
            {
                luch.material = new Material(shader);
            }
        }

        materialLuchaKopiya = luch.material;

        if (materialPrizraka == null)
        {
            Debug.LogWarning("PosadkaDereva: materialPrizraka не назначен - призрак будет непрозрачным.");
        }

        if (naborDeistvii == null)
        {
            Debug.LogWarning("PosadkaDereva: naborDeistvii не назначен!");
            return;
        }

        var karta = naborDeistvii.FindActionMap(imyaKarty);
        if (karta != null)
        {
            deistvieTriggera = karta.FindAction(imyaDeistviya);
        }

        if (deistvieTriggera == null)
        {
            Debug.LogWarning("PosadkaDereva: действие '" + imyaDeistviya + "' не найдено в карте '" + imyaKarty + "'.");
        }
        else
        {
            Debug.Log("PosadkaDereva: действие '" + imyaDeistviya + "' найдено успешно.");
        }
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
        if (deistvieTriggera != null)
        {
            deistvieTriggera.performed -= NazhatieTriggera;
        }

        if (luch != null)
        {
            luch.enabled = false;
        }

        SpryatPrizrak();
    }

    void OnDestroy()
    {
        UnichtozhitPrizrak();
    }

    // LateUpdate - чтобы позиция контроллера уже обновилась в этом кадре
    void LateUpdate()
    {
        GameObject prefab = VyborInstrumenta.TekushiyPrefab;
        bool menyuMeshaet = SostoyanieMenyu.AnyMenuOpen && !rabotatPriOtkrytomMenyu;

        luchAktiven = (prefab != null) && !menyuMeshaet;
        popalNaTerrein = false;

        if (!luchAktiven)
        {
            luch.enabled = false;
            SpryatPrizrak();
            return;
        }

        Transform start = tochkaStarta != null ? tochkaStarta : transform;
        Vector3 nachalo = start.position;
        Vector3 napravlenie = start.forward;
        Vector3 konets = nachalo + napravlenie * dlinaLucha;

        RaycastHit popadanie;
        if (Physics.Raycast(nachalo, napravlenie, out popadanie, dlinaLucha, maskaTerreina, QueryTriggerInteraction.Ignore))
        {
            popalNaTerrein = true;
            tochkaPopadaniya = popadanie.point;
            konets = popadanie.point;
            PokazatPrizrak(prefab, popadanie.point);
        }
        else
        {
            SpryatPrizrak();
        }

        luch.enabled = true;
        luch.SetPosition(0, nachalo);
        luch.SetPosition(1, konets);
        UstanovitTsvetLucha(popalNaTerrein ? tsvetPopal : tsvetNePopal);
    }

    private void NazhatieTriggera(InputAction.CallbackContext context)
    {
        if (!luchAktiven || !popalNaTerrein)
        {
            return;
        }

        GameObject prefab = VyborInstrumenta.TekushiyPrefab;
        if (prefab == null)
        {
            return;
        }

        GameObject novoe = Instantiate(prefab, tochkaPopadaniya, Quaternion.identity, kontejnerDerevev);
        Debug.Log("Посажено: " + novoe.name);
    }

    private void UstanovitTsvetLucha(Color tsvet)
    {
        if (materialLuchaKopiya == null)
        {
            return;
        }

        if (materialLuchaKopiya.HasProperty("_BaseColor"))
        {
            materialLuchaKopiya.SetColor("_BaseColor", tsvet);
        }
        else if (materialLuchaKopiya.HasProperty("_Color"))
        {
            materialLuchaKopiya.SetColor("_Color", tsvet);
        }
    }

    private void PokazatPrizrak(GameObject prefab, Vector3 pozitsiya)
    {
        // Выбрали другое дерево - делаем новый призрак
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

    private void SpryatPrizrak()
    {
        if (prizrak != null)
        {
            prizrak.SetActive(false);
        }
    }

    private void SozdatPrizrak(GameObject prefab)
    {
        prefabPrizraka = prefab;
        prizrak = Instantiate(prefab);
        prizrak.name = prefab.name + " (prizrak)";

        // У призрака не должно быть коллайдеров, чтобы он ни с чем не взаимодействовал
        Collider[] kollajdery = prizrak.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < kollajdery.Length; i++)
        {
            kollajdery[i].enabled = false;
        }

        // Заменяем все материалы на один прозрачный
        if (materialPrizraka != null)
        {
            Renderer[] otrisovshiki = prizrak.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < otrisovshiki.Length; i++)
            {
                Material[] novyeMaterialy = new Material[otrisovshiki[i].sharedMaterials.Length];
                for (int j = 0; j < novyeMaterialy.Length; j++)
                {
                    novyeMaterialy[j] = materialPrizraka;
                }

                otrisovshiki[i].sharedMaterials = novyeMaterialy;
            }
        }
    }

    private void UnichtozhitPrizrak()
    {
        if (prizrak != null)
        {
            Destroy(prizrak);
        }

        prizrak = null;
        prefabPrizraka = null;
    }
}