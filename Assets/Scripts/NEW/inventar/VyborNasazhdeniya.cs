using UnityEngine;
using UnityEngine.InputSystem;

// Вешается на пустой объект внутри Right Controller (LineRenderer добавится сам).
// Пока инвентарь открыт: рисует луч от правой руки, наведённое посаженное
// насаждение подсвечивается, правый триггер выбирает его в инвентаре.
[RequireComponent(typeof(LineRenderer))]
public class VyborNasazhdeniya : MonoBehaviour
{
    [Header("Панель действий инвентаря (на ней висит InventarDeystviy)")]
    public InventarDeystviy inventar;

    [Header("Откуда идёт луч (если пусто - сам этот объект)")]
    public Transform tochkaStarta;

    [Header("Какие слои луч может задеть")]
    public LayerMask maskaPoiska = ~0;

    [Header("Длина луча")]
    public float dlinaLucha = 30f;

    [Header("Материал луча (необязательно, лучше URP/Unlit) и красный материал подсветки")]
    public Material materialLucha;
    public Material materialPodsvetki;

    [Header("Цвет луча: навели на насаждение / ничего нет")]
    public Color tsvetNaNasazhdenii = Color.red;
    public Color tsvetPustoi = Color.white;

    [Header("Весь asset с действиями")]
    public InputActionAsset naborDeistvii;

    [Header("Карта и имя действия правого триггера (Button)")]
    public string imyaKarty = "XRI Right Interaction";
    public string imyaDeistviya = "PickTool";

    private LineRenderer luch;
    private Material materialLuchaKopiya;
    private InputAction deistvieTriggera;

    private Nasazhdenie navedennoe;
    private Nasazhdenie vybrannoe;

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

        if (materialPodsvetki == null)
        {
            Debug.LogWarning("VyborNasazhdeniya: materialPodsvetki не назначен - подсветки не будет.");
        }

        if (inventar == null)
        {
            Debug.LogWarning("VyborNasazhdeniya: inventar не назначен!");
        }

        if (naborDeistvii == null)
        {
            Debug.LogWarning("VyborNasazhdeniya: naborDeistvii не назначен!");
            return;
        }

        var karta = naborDeistvii.FindActionMap(imyaKarty);
        if (karta != null)
        {
            deistvieTriggera = karta.FindAction(imyaDeistviya);
        }

        if (deistvieTriggera == null)
        {
            Debug.LogWarning("VyborNasazhdeniya: действие '" + imyaDeistviya + "' не найдено в карте '" + imyaKarty + "'.");
        }
        else
        {
            Debug.Log("VyborNasazhdeniya: действие '" + imyaDeistviya + "' найдено успешно.");
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
    }

    // LateUpdate - чтобы позиция контроллера уже обновилась в этом кадре
    void LateUpdate()
    {
        // Объект могли удалить кнопкой "Удалить" - тогда ссылки станут пустыми
        if (vybrannoe == null)
        {
            vybrannoe = null;
        }

        if (!SostoyanieMenyu.InventoryOpen)
        {
            ZakrytInventar();
            return;
        }

        Transform start = tochkaStarta != null ? tochkaStarta : transform;
        Vector3 nachalo = start.position;
        Vector3 napravlenie = start.forward;
        Vector3 konets = nachalo + napravlenie * dlinaLucha;

        Nasazhdenie naydennoe = null;

        RaycastHit popadanie;
        if (Physics.Raycast(nachalo, napravlenie, out popadanie, dlinaLucha, maskaPoiska, QueryTriggerInteraction.Ignore))
        {
            konets = popadanie.point;
            naydennoe = popadanie.collider.GetComponentInParent<Nasazhdenie>();
        }

        ObnovitNavedenie(naydennoe);

        luch.enabled = true;
        luch.SetPosition(0, nachalo);
        luch.SetPosition(1, konets);
        UstanovitTsvetLucha(naydennoe != null ? tsvetNaNasazhdenii : tsvetPustoi);
    }

    // Инвентарь закрыли: прячем луч, снимаем подсветку и забываем выбор
    private void ZakrytInventar()
    {
        luch.enabled = false;

        SnyatPodsvetku(navedennoe);
        SnyatPodsvetku(vybrannoe);
        navedennoe = null;

        if (vybrannoe != null)
        {
            vybrannoe = null;
            if (inventar != null)
            {
                inventar.UstanovitTekushiyObyekt(null);
            }
        }
    }

    private void ObnovitNavedenie(Nasazhdenie novoe)
    {
        if (novoe == navedennoe)
        {
            return;
        }

        // Убираем подсветку со старого (кроме выбранного - оно остаётся красным)
        if (navedennoe != null && navedennoe != vybrannoe)
        {
            SnyatPodsvetku(navedennoe);
        }

        navedennoe = novoe;
        Podsvetit(navedennoe);
    }

    private void NazhatieTriggera(InputAction.CallbackContext context)
    {
        if (!SostoyanieMenyu.InventoryOpen || navedennoe == null)
        {
            return;
        }

        // Старое выбранное перестаёт быть красным (если на него не наведён луч)
        if (vybrannoe != null && vybrannoe != navedennoe)
        {
            SnyatPodsvetku(vybrannoe);
        }

        vybrannoe = navedennoe;

        if (inventar != null)
        {
            inventar.UstanovitTekushiyObyekt(vybrannoe);
        }

        Debug.Log("Выбрано насаждение: " + vybrannoe.vidNasazhdeniya);
    }

    private void Podsvetit(Nasazhdenie nasazhdenie)
    {
        if (nasazhdenie == null)
        {
            return;
        }

        PodsvetkaNasazhdeniya podsvetka = nasazhdenie.GetComponent<PodsvetkaNasazhdeniya>();
        if (podsvetka == null)
        {
            podsvetka = nasazhdenie.gameObject.AddComponent<PodsvetkaNasazhdeniya>();
        }

        podsvetka.Vkluchit(materialPodsvetki);
    }

    private void SnyatPodsvetku(Nasazhdenie nasazhdenie)
    {
        if (nasazhdenie == null)
        {
            return;
        }

        PodsvetkaNasazhdeniya podsvetka = nasazhdenie.GetComponent<PodsvetkaNasazhdeniya>();
        if (podsvetka != null)
        {
            podsvetka.Vyklyuchit();
        }
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
}