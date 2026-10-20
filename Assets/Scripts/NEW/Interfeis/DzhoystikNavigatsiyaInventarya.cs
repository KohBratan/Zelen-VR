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
// Дополнительно: режим "Заменить вид". Пока он включён, джойстик листает
// кнопки видов в отдельной панели. Горизонтальный свайп "за край" сетки
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

    [Header("=== Панель 'Заменить вид' ===")]
    [Header("Панель замены целиком (открывается кнопкой 'Заменить вид')")]
    public GameObject panelZameny;

    [Header("Кнопки видов в панели замены (KnopkaZamenyVida), по порядку")]
    public KnopkaZamenyVida[] knopkiZameny;

    [Header("Сколько колонок в панели замены (1 = вертикальный список)")]
    public int kolonokZameny = 2;

    [Header("Кнопка 'Заменить вид' в инвентаре (к ней возвращаемся после закрытия панели)")]
    public Button knopkaZamenitVid;

    [Header("Куда складывать новые деревья после замены (необязательно)")]
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
                if (knopkiZameny != null && tekushiyIndeksZameny >= 0 && tekushiyIndeksZameny < knopkiZameny.Length && knopkiZameny[tekushiyIndeksZameny] != null)
                {
                    EventSystem.current.SetSelectedGameObject(knopkiZameny[tekushiyIndeksZameny].gameObject);
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
    // Если панель - сетка (kolonokZameny > 1):
    //   вверх/вниз - по колонке, влево/вправо - по ряду;
    //   свайп "за край" (влево с левой колонки, вправо с правой) - выход в инвентарь.
    // Если список (kolonokZameny <= 1):
    //   вверх/вниз - по списку, любой горизонтальный свайп - выход.
    private void ObrabotatNaklonZameny(Vector2 napravlenie)
    {
        bool eto_gorizontalniy = Mathf.Abs(napravlenie.x) > Mathf.Abs(napravlenie.y);

        bool etoSetka = kolonokZameny > 1;

        if (eto_gorizontalniy)
        {
            if (etoSetka)
            {
                bool vpravo = napravlenie.x > 0;
                bool sdvinulis = ShagPoRyaduZameny(vpravo ? 1 : -1);

                // Не сдвинулись (крайняя колонка) - выходим в инвентарь
                if (!sdvinulis)
                {
                    ZakrytPanelZameny();
                }
            }
            else
            {
                // Обычный список: любой горизонтальный свайп - выход
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
        if (knopkiZameny == null || knopkiZameny.Length == 0)
        {
            return;
        }

        int noviy = (tekushiyIndeksZameny + napravlenie + knopkiZameny.Length) % knopkiZameny.Length;
        VybratElementZameny(noviy);
    }

    private void ShagPoKolonkeZameny(int napravlenie)
    {
        int vsego = knopkiZameny.Length;
        if (vsego == 0)
        {
            return;
        }

        int kolonka = tekushiyIndeksZameny % kolonokZameny;
        int stroka = tekushiyIndeksZameny / kolonokZameny;
        int strokVsego = (vsego + kolonokZameny - 1) / kolonokZameny;

        int noviy = tekushiyIndeksZameny;

        for (int popytka = 0; popytka < strokVsego; popytka++)
        {
            stroka = (stroka + napravlenie + strokVsego) % strokVsego;
            int kandidat = stroka * kolonokZameny + kolonka;

            if (kandidat < vsego)
            {
                noviy = kandidat;
                break;
            }
        }

        VybratElementZameny(noviy);
    }

    // Шаг влево/вправо по ряду. Возвращает true, если курсор реально сдвинулся.
    private bool ShagPoRyaduZameny(int napravlenie)
    {
        int vsego = knopkiZameny.Length;
        if (vsego == 0)
        {
            return false;
        }

        int kolonka = tekushiyIndeksZameny % kolonokZameny;
        int kandidat = tekushiyIndeksZameny + napravlenie;

        if (kandidat < 0 || kandidat >= vsego)
        {
            return false;
        }

        int kandidatKolonka = kandidat % kolonokZameny;
        if (kandidatKolonka == 0 && napravlenie > 0)
        {
            // Перескочили на новую строку
            return false;
        }
        if (kandidatKolonka == kolonokZameny - 1 && napravlenie < 0)
        {
            // Перескочили на предыдущую строку справа
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

        if (knopkiZameny[indeks] != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(knopkiZameny[indeks].gameObject);
            Debug.Log("Замена вида: выбран '" + knopkiZameny[indeks].name + "'");
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
            if (knopkiZameny != null && tekushiyIndeksZameny >= 0 && tekushiyIndeksZameny < knopkiZameny.Length && knopkiZameny[tekushiyIndeksZameny] != null)
            {
                knopkiZameny[tekushiyIndeksZameny].Nazhat();
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

    public void OtkrytPanelZameny(Nasazhdenie tekushiy)
    {
        if (panelZameny == null || knopkiZameny == null || knopkiZameny.Length == 0)
        {
            Debug.LogWarning("DzhoystikNavigatsiyaInventarya: панель замены не настроена в инспекторе.");
            return;
        }

        zamenyaemyiObyekt = tekushiy;
        rezhimZameny = true;
        panelZameny.SetActive(true);

        int indeksTekushchego = 0;
        if (tekushiy != null && tekushiy.sourcePrefab != null)
        {
            for (int i = 0; i < knopkiZameny.Length; i++)
            {
                if (knopkiZameny[i] != null && knopkiZameny[i].prefabNasazhdeniya == tekushiy.sourcePrefab)
                {
                    indeksTekushchego = i;
                    break;
                }
            }
        }

        for (int i = 0; i < knopkiZameny.Length; i++)
        {
            if (knopkiZameny[i] != null)
            {
                knopkiZameny[i].UstanovitMetku(i == indeksTekushchego);
            }
        }

        tekushiyIndeksZameny = indeksTekushchego;

        if (knopkiZameny[indeksTekushchego] != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(knopkiZameny[indeksTekushchego].gameObject);
        }

        Debug.Log("Панель замены открыта для: " + (tekushiy != null ? tekushiy.vidNasazhdeniya : "?"));
    }

    public void ZakrytPanelZameny()
    {
        rezhimZameny = false;
        zamenyaemyiObyekt = null;

        if (panelZameny != null)
        {
            panelZameny.SetActive(false);
        }

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

// Общий "флаг состояния меню". Оба скрипта навигации пишут сюда своё состояние,
// а скрипт инвентаря в LateUpdate читает AnyMenuOpen и переключает локомоцию.
public static class SostoyanieMenyu
{
    public static bool InventoryOpen;
    public static bool ToolsMenuOpen;

    public static bool AnyMenuOpen => InventoryOpen || ToolsMenuOpen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Sbrosit()
    {
        InventoryOpen = false;
        ToolsMenuOpen = false;
    }
}