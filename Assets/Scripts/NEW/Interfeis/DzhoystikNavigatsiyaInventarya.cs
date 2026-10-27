using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Пока инвентарь открыт, правый джойстик перестаёт поворачивать камеру
// и начинает переключать элементы списка (кнопки и слайдеры).
//
// Вверх/вниз - следующий/предыдущий элемент списка
// Влево/вправо - если выбран слайдер, двигает его значение на шаг
// Клик стика - если выбрана кнопка, нажимает её
//
// Дополнительно: режим "Заменить вид". Панелей замены может быть несколько
// (сейчас - для деревьев и для кустов). Какую открыть, решается по
// kategoriya текущего объекта. Горизонтальный свайп "за край" сетки
// (влево с левой колонки, вправо с правой) возвращает в основной инвентарь.
//
// ВАЖНО: этот скрипт также является "арбитром" для отключения локомоции.
// Он один (в LateUpdate) включает/выключает komponentPovorota и
// komponentPeredvizheniya в зависимости от того, открыто ли ЛЮБОЕ из меню.
public class DzhoystikNavigatsiyaInventarya : MonoBehaviour
{
    [Header("Панель инвентаря целиком (чтобы понять, открыта ли она)")]
    public GameObject panelMenu;

    [Header("Элементы списка по порядку (кнопки и слайдеры)")]
    public Selectable[] elementyMenu;

    [Header("Шаг изменения слайдера за один наклон влево/вправо")]
    public float shagSlaidera = 0.1f;

    [Header("Компонент поворота камеры, который нужно выключать, пока любое меню открыто")]
    public Behaviour komponentPovorota;

    [Header("Компонент передвижения персонажа, который нужно выключать, пока любое меню открыто")]
    public Behaviour komponentPeredvizheniya;

    [Header("Весь asset с действиями")]
    public InputActionAsset naborDeistvii;

    [Header("Карта и имя действия для самого джойстика (Vector2)")]
    public string imyaKartyDzhoystika = "XRI Right Interaction";
    public string imyaDeistviyaDzhoystika = "MenuJoystick";

    [Header("Карта и имя действия для клика стиком (кнопка)")]
    public string imyaKartyKlika = "XRI Right Interaction";
    public string imyaDeistviyaKlika = "MenuConfirm";

    [Header("Порог наклона, после которого засчитывается шаг")]
    public float porogNaklona = 0.5f;

    [Header("Диагностика: показывать сырые значения джойстика в консоли")]
    public bool pokazyvatSyrieZnacheniya = false;

    [Header("=== Панель 'Заменить вид' для ДЕРЕВЬЕВ ===")]
    public GameObject panelZamenyDerevya;
    public KnopkaZamenyVida[] knopkiZamenyDerevya;
    public int kolonokZamenyDerevya = 2;

    [Header("=== Панель 'Заменить вид' для КУСТОВ ===")]
    public GameObject panelZamenyKusty;
    public KnopkaZamenyVida[] knopkiZamenyKusty;
    public int kolonokZamenyKusty = 2;

    [Header("Кнопка 'Заменить вид' в инвентаре (к ней возвращаемся после закрытия панели)")]
    public Button knopkaZamenitVid;

    [Header("Куда складывать новые объекты после замены (необязательно)")]
    public Transform kontejnerDerevev;

    [Header("Ссылка на панель инвентаря (чтобы обновить выделенный объект после замены)")]
    public InventarDeystviy inventar;

    private InputAction deistvieDzhoystika;
    private InputAction deistvieKlika;

    private int tekushiyIndeks = 0;
    private bool zhdyomNeytral = false;
    private bool panelByloOtkrytoProshlyiKadr = false;

    // Режим замены вида
    private bool rezhimZameny = false;
    private Nasazhdenie zamenyaemyiObyekt = null;
    private int tekushiyIndeksZameny = 0;

    // Активный набор (выбирается при открытии панели замены)
    private GameObject tekushayaPanelZameny;
    private KnopkaZamenyVida[] tekushieKnopkiZameny;
    private int tekushieKolonkiZameny = 2;

    void Awake()
    {
        if (naborDeistvii == null)
        {
            Debug.LogWarning("DzhoystikNavigatsiyaInventarya: naborDeistvii не назначен!");
            return;
        }

        var kartaDzhoystika = naborDeistvii.FindActionMap(imyaKartyDzhoystika);
        if (kartaDzhoystika != null)
        {
            deistvieDzhoystika = kartaDzhoystika.FindAction(imyaDeistviyaDzhoystika);
        }

        var kartaKlika = naborDeistvii.FindActionMap(imyaKartyKlika);
        if (kartaKlika != null)
        {
            deistvieKlika = kartaKlika.FindAction(imyaDeistviyaKlika);
        }

        if (deistvieDzhoystika == null)
        {
            Debug.LogWarning("DzhoystikNavigatsiyaInventarya: действие '" + imyaDeistviyaDzhoystika + "' не найдено в карте '" + imyaKartyDzhoystika + "'.");
        }
        else
        {
            Debug.Log("DzhoystikNavigatsiyaInventarya: действие джойстика '" + imyaDeistviyaDzhoystika + "' найдено успешно.");
        }

        if (deistvieKlika == null)
        {
            Debug.LogWarning("DzhoystikNavigatsiyaInventarya: действие '" + imyaDeistviyaKlika + "' не найдено в карте '" + imyaKartyKlika + "'.");
        }
        else
        {
            Debug.Log("DzhoystikNavigatsiyaInventarya: действие клика '" + imyaDeistviyaKlika + "' найдено успешно.");
        }
    }

    void OnEnable()
    {
        if (deistvieDzhoystika != null)
        {
            deistvieDzhoystika.performed += SobytieDzhoystika;
            deistvieDzhoystika.canceled += SobytieDzhoystikaSbros;
            deistvieDzhoystika.Enable();
        }

        if (deistvieKlika != null)
        {
            deistvieKlika.performed += NazhatieKlika;
            deistvieKlika.Enable();
        }
    }

    void OnDisable()
    {
        if (deistvieDzhoystika != null)
        {
            deistvieDzhoystika.performed -= SobytieDzhoystika;
            deistvieDzhoystika.canceled -= SobytieDzhoystikaSbros;
        }

        if (deistvieKlika != null)
        {
            deistvieKlika.performed -= NazhatieKlika;
        }
    }

    void Update()
    {
        bool menyuOtkryto = panelMenu != null && panelMenu.activeInHierarchy;

        SostoyanieMenyu.InventoryOpen = menyuOtkryto;

        if (!menyuOtkryto)
        {
            zhdyomNeytral = false;
            panelByloOtkrytoProshlyiKadr = false;

            if (rezhimZameny)
            {
                ZakrytPanelZameny();
            }

            return;
        }

        if (rezhimZameny)
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                if (tekushieKnopkiZameny != null && tekushiyIndeksZameny >= 0 && tekushiyIndeksZameny < tekushieKnopkiZameny.Length && tekushieKnopkiZameny[tekushiyIndeksZameny] != null)
                {
                    EventSystem.current.SetSelectedGameObject(tekushieKnopkiZameny[tekushiyIndeksZameny].gameObject);
                }
            }
            return;
        }

        if (!panelByloOtkrytoProshlyiKadr)
        {
            VybratElementPoIndeksu(NaytiPervyiDostupniyIndeks());
        }
        else if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            VybratElementPoIndeksu(tekushiyIndeks);
        }

        panelByloOtkrytoProshlyiKadr = menyuOtkryto;
    }

    void LateUpdate()
    {
        bool blokirovat = SostoyanieMenyu.AnyMenuOpen;

        if (komponentPovorota != null)
        {
            komponentPovorota.enabled = !blokirovat;
        }

        if (komponentPeredvizheniya != null)
        {
            komponentPeredvizheniya.enabled = !blokirovat;
        }
    }

    private void SobytieDzhoystika(InputAction.CallbackContext context)
    {
        bool menyuOtkryto = panelMenu != null && panelMenu.activeInHierarchy;

        Vector2 znachenie = context.ReadValue<Vector2>();

        if (pokazyvatSyrieZnacheniya)
        {
            Debug.Log("Инвентарь: событие джойстика, значение = " + znachenie + " (меню открыто: " + menyuOtkryto + ")");
        }

        if (!menyuOtkryto)
        {
            return;
        }

        float velichina = znachenie.magnitude;

        if (!zhdyomNeytral && velichina > porogNaklona)
        {
            ObrabotatNaklon(znachenie);
            zhdyomNeytral = true;
        }
    }

    private void SobytieDzhoystikaSbros(InputAction.CallbackContext context)
    {
        zhdyomNeytral = false;
    }

    private void ObrabotatNaklon(Vector2 napravlenie)
    {
        if (rezhimZameny)
        {
            ObrabotatNaklonZameny(napravlenie);
            return;
        }

        bool eto_gorizontalniy_naklon = Mathf.Abs(napravlenie.x) > Mathf.Abs(napravlenie.y);

        if (eto_gorizontalniy_naklon)
        {
            IzmenitSlaiderEsliVybran(napravlenie.x > 0 ? shagSlaidera : -shagSlaidera);
        }
        else
        {
            if (napravlenie.y > 0)
            {
                PereklyuchitElement(-1);
            }
            else
            {
                PereklyuchitElement(1);
            }
        }
    }

    // Наклон джойстика, пока открыта панель замены.
    // Если панель - сетка (tekushieKolonkiZameny > 1):
    //   вверх/вниз - по колонке, влево/вправо - по ряду;
    //   свайп "за край" (влево с левой колонки, вправо с правой) - выход в инвентарь.
    // Если список (tekushieKolonkiZameny <= 1):
    //   вверх/вниз - по списку, любой горизонтальный свайп - выход.
    private void ObrabotatNaklonZameny(Vector2 napravlenie)
    {
        bool eto_gorizontalniy = Mathf.Abs(napravlenie.x) > Mathf.Abs(napravlenie.y);
        bool etoSetka = tekushieKolonkiZameny > 1;

        if (eto_gorizontalniy)
        {
            if (etoSetka)
            {
                bool vpravo = napravlenie.x > 0;
                bool sdvinulis = ShagPoRyaduZameny(vpravo ? 1 : -1);

                if (!sdvinulis)
                {
                    ZakrytPanelZameny();
                }
            }
            else
            {
                ZakrytPanelZameny();
            }
            return;
        }

        int shag = napravlenie.y > 0 ? -1 : 1;

        if (etoSetka)
        {
            ShagPoKolonkeZameny(shag);
        }
        else
        {
            PereklyuchitElementZameny(shag);
        }
    }

    private void PereklyuchitElement(int napravlenie)
    {
        if (elementyMenu == null || elementyMenu.Length == 0)
        {
            return;
        }

        int noviyIndeks = tekushiyIndeks;

        for (int popytka = 0; popytka < elementyMenu.Length; popytka++)
        {
            noviyIndeks = (noviyIndeks + napravlenie + elementyMenu.Length) % elementyMenu.Length;

            if (ElementDostupen(noviyIndeks))
            {
                VybratElementPoIndeksu(noviyIndeks);
                return;
            }
        }
    }

    private void PereklyuchitElementZameny(int napravlenie)
    {
        if (tekushieKnopkiZameny == null || tekushieKnopkiZameny.Length == 0)
        {
            return;
        }

        int noviy = (tekushiyIndeksZameny + napravlenie + tekushieKnopkiZameny.Length) % tekushieKnopkiZameny.Length;
        VybratElementZameny(noviy);
    }

    private void ShagPoKolonkeZameny(int napravlenie)
    {
        int vsego = tekushieKnopkiZameny.Length;
        if (vsego == 0)
        {
            return;
        }

        int kolonka = tekushiyIndeksZameny % tekushieKolonkiZameny;
        int stroka = tekushiyIndeksZameny / tekushieKolonkiZameny;
        int strokVsego = (vsego + tekushieKolonkiZameny - 1) / tekushieKolonkiZameny;

        int noviy = tekushiyIndeksZameny;

        for (int popytka = 0; popytka < strokVsego; popytka++)
        {
            stroka = (stroka + napravlenie + strokVsego) % strokVsego;
            int kandidat = stroka * tekushieKolonkiZameny + kolonka;

            if (kandidat < vsego)
            {
                noviy = kandidat;
                break;
            }
        }

        VybratElementZameny(noviy);
    }

    private bool ShagPoRyaduZameny(int napravlenie)
    {
        int vsego = tekushieKnopkiZameny.Length;
        if (vsego == 0)
        {
            return false;
        }

        int kolonka = tekushiyIndeksZameny % tekushieKolonkiZameny;
        int kandidat = tekushiyIndeksZameny + napravlenie;

        if (kandidat < 0 || kandidat >= vsego)
        {
            return false;
        }

        int kandidatKolonka = kandidat % tekushieKolonkiZameny;
        if (kandidatKolonka == 0 && napravlenie > 0)
        {
            return false;
        }
        if (kandidatKolonka == tekushieKolonkiZameny - 1 && napravlenie < 0)
        {
            return false;
        }

        if (Mathf.Abs(kandidatKolonka - kolonka) != 1)
        {
            return false;
        }

        VybratElementZameny(kandidat);
        return true;
    }

    private void VybratElementZameny(int indeks)
    {
        tekushiyIndeksZameny = indeks;

        if (tekushieKnopkiZameny[indeks] != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(tekushieKnopkiZameny[indeks].gameObject);
            Debug.Log("Замена вида: выбран '" + tekushieKnopkiZameny[indeks].name + "'");
        }
    }

    private bool ElementDostupen(int indeks)
    {
        Selectable element = elementyMenu[indeks];
        return element != null && element.gameObject.activeInHierarchy && element.interactable;
    }

    private int NaytiPervyiDostupniyIndeks()
    {
        for (int i = 0; i < elementyMenu.Length; i++)
        {
            if (ElementDostupen(i))
            {
                return i;
            }
        }

        return 0;
    }

    private void VybratElementPoIndeksu(int indeks)
    {
        tekushiyIndeks = indeks;

        if (elementyMenu[indeks] != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(elementyMenu[indeks].gameObject);
            Debug.Log("Инвентарь: выбран элемент '" + elementyMenu[indeks].name + "'");
        }
        else if (EventSystem.current == null)
        {
            Debug.LogWarning("Инвентарь: в сцене нет EventSystem, подсветка выбора работать не будет.");
        }
    }

    private void IzmenitSlaiderEsliVybran(float izmenenie)
    {
        if (elementyMenu == null || tekushiyIndeks >= elementyMenu.Length)
        {
            return;
        }

        Slider slaider = elementyMenu[tekushiyIndeks] as Slider;
        if (slaider != null)
        {
            slaider.value += izmenenie;
        }
    }

    private void NazhatieKlika(InputAction.CallbackContext context)
    {
        if (rezhimZameny)
        {
            if (tekushieKnopkiZameny != null && tekushiyIndeksZameny >= 0 && tekushiyIndeksZameny < tekushieKnopkiZameny.Length && tekushieKnopkiZameny[tekushiyIndeksZameny] != null)
            {
                tekushieKnopkiZameny[tekushiyIndeksZameny].Nazhat();
            }
            return;
        }

        if (elementyMenu == null || tekushiyIndeks >= elementyMenu.Length)
        {
            return;
        }

        Button knopka = elementyMenu[tekushiyIndeks] as Button;
        if (knopka != null && knopka.interactable)
        {
            knopka.onClick.Invoke();
        }
    }

    // ===== Публичное API для панели замены вида =====

    // Открыть панель замены для указанного объекта. Какую именно панель
    // (деревья или кусты) - определяется по kategoriya.
    public void OtkrytPanelZameny(Nasazhdenie tekushiy)
    {
        if (tekushiy == null)
        {
            Debug.LogWarning("DzhoystikNavigatsiyaInventarya: нет объекта для замены вида.");
            return;
        }

        // Выбираем набор по категории
        GameObject vybrannayaPanel = null;
        KnopkaZamenyVida[] vybrannyeKnopki = null;
        int vybrannyeKolonki = 2;

        if (tekushiy.kategoriya == "Деревья")
        {
            vybrannayaPanel = panelZamenyDerevya;
            vybrannyeKnopki = knopkiZamenyDerevya;
            vybrannyeKolonki = kolonokZamenyDerevya;
        }
        else if (tekushiy.kategoriya == "Кусты")
        {
            vybrannayaPanel = panelZamenyKusty;
            vybrannyeKnopki = knopkiZamenyKusty;
            vybrannyeKolonki = kolonokZamenyKusty;
        }
        else
        {
            Debug.LogWarning("DzhoystikNavigatsiyaInventarya: замена вида для категории '" + tekushiy.kategoriya + "' не настроена.");
            return;
        }

        if (vybrannayaPanel == null || vybrannyeKnopki == null || vybrannyeKnopki.Length == 0)
        {
            Debug.LogWarning("DzhoystikNavigatsiyaInventarya: панель замены для категории '" + tekushiy.kategoriya + "' не заполнена в инспекторе.");
            return;
        }

        // Запоминаем активный набор
        tekushayaPanelZameny = vybrannayaPanel;
        tekushieKnopkiZameny = vybrannyeKnopki;
        tekushieKolonkiZameny = Mathf.Max(1, vybrannyeKolonki);

        zamenyaemyiObyekt = tekushiy;
        rezhimZameny = true;
        tekushayaPanelZameny.SetActive(true);

        // Ищем кнопку с тем же префабом, что и у текущего дерева
        int indeksTekushchego = 0;
        if (tekushiy.sourcePrefab != null)
        {
            for (int i = 0; i < tekushieKnopkiZameny.Length; i++)
            {
                if (tekushieKnopkiZameny[i] != null && tekushieKnopkiZameny[i].prefabNasazhdeniya == tekushiy.sourcePrefab)
                {
                    indeksTekushchego = i;
                    break;
                }
            }
        }

        // Галочка - только на текущем виде
        for (int i = 0; i < tekushieKnopkiZameny.Length; i++)
        {
            if (tekushieKnopkiZameny[i] != null)
            {
                tekushieKnopkiZameny[i].UstanovitMetku(i == indeksTekushchego);
            }
        }

        tekushiyIndeksZameny = indeksTekushchego;

        if (tekushieKnopkiZameny[indeksTekushchego] != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(tekushieKnopkiZameny[indeksTekushchego].gameObject);
        }

        Debug.Log("Панель замены (" + tekushiy.kategoriya + ") открыта для: " + tekushiy.vidNasazhdeniya);
    }

    public void ZakrytPanelZameny()
    {
        rezhimZameny = false;
        zamenyaemyiObyekt = null;

        if (tekushayaPanelZameny != null)
        {
            tekushayaPanelZameny.SetActive(false);
        }

        // На случай, если вторая панель осталась включённой (подстраховка)
        if (panelZamenyDerevya != null && panelZamenyDerevya != tekushayaPanelZameny)
        {
            panelZamenyDerevya.SetActive(false);
        }
        if (panelZamenyKusty != null && panelZamenyKusty != tekushayaPanelZameny)
        {
            panelZamenyKusty.SetActive(false);
        }

        tekushayaPanelZameny = null;
        tekushieKnopkiZameny = null;

        bool menyuOtkryto = panelMenu != null && panelMenu.activeInHierarchy;
        if (menyuOtkryto && knopkaZamenitVid != null && elementyMenu != null)
        {
            for (int i = 0; i < elementyMenu.Length; i++)
            {
                if (elementyMenu[i] == knopkaZamenitVid)
                {
                    VybratElementPoIndeksu(i);
                    return;
                }
            }
        }
    }

    public void PrimenitZamenu(KnopkaZamenyVida knopka)
    {
        if (knopka == null || knopka.prefabNasazhdeniya == null)
        {
            return;
        }

        if (zamenyaemyiObyekt != null)
        {
            Transform t = zamenyaemyiObyekt.transform;
            Vector3 pos = t.position;
            Quaternion rot = t.rotation;
            Vector3 scale = t.localScale;
            bool zafiks = zamenyaemyiObyekt.zafiksirovano;

            GameObject novoe = Instantiate(knopka.prefabNasazhdeniya, pos, rot, kontejnerDerevev);
            novoe.transform.localScale = scale;

            Nasazhdenie nas = novoe.GetComponent<Nasazhdenie>();
            if (nas != null)
            {
                nas.sourcePrefab = knopka.prefabNasazhdeniya;
                nas.zafiksirovano = zafiks;
            }

            Destroy(zamenyaemyiObyekt.gameObject);

            if (inventar != null && nas != null)
            {
                inventar.UstanovitTekushiyObyekt(nas);
            }

            Debug.Log("Заменено на вид: " + (nas != null ? nas.vidNasazhdeniya : novoe.name));
        }

        ZakrytPanelZameny();
    }
}

// Общий "флаг состояния меню". Навигация и игровое меню пишут сюда своё состояние,
// а скрипт инвентаря в LateUpdate читает AnyMenuOpen и переключает локомоцию.
public static class SostoyanieMenyu
{
    public static bool InventoryOpen;
    public static bool ToolsMenuOpen;
    public static bool GameMenuOpen;

    public static bool AnyMenuOpen => InventoryOpen || ToolsMenuOpen || GameMenuOpen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Sbrosit()
    {
        InventoryOpen = false;
        ToolsMenuOpen = false;
        GameMenuOpen = false;
    }
}