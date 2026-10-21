using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

// Пока меню открыто, левый джойстик перестаёт двигать персонажа
// и начинает управлять списком категорий и списком видов внутри категории.
//
// Вверх/вниз - листание текущего списка (в сетке - по колонке)
// Вправо     - зайти внутрь выбранной категории (в сетке внутри категории - шаг вправо по ряду)
// Влево      - в сетке шаг влево по ряду; из левой колонки (или из обычного списка) - выйти к списку категорий
// Клик стика - подтвердить выбранный вид
//
// ВАЖНО: этот скрипт НЕ трогает enabled компонентов локомоции напрямую -
// он только сообщает своё состояние в SostoyanieMenyu. Переключением
// занимается DzhoystikNavigatsiyaInventarya (в LateUpdate).
public class DzhoystikNavigatsiyaMenu : MonoBehaviour
{
    [Header("Панель меню целиком (используется, чтобы понять, открыто ли меню)")]
    public GameObject panelMenu;

    [Header("Меню верхнего уровня — список категорий")]
    public MenuVkladki menuKategoriy;

    [Header("Меню видов внутри каждой категории (тот же порядок, что и категории)")]
    public MenuVkladki[] menuVidovPoKategoriyam;

    [Header("Весь asset с действиями")]
    public InputActionAsset naborDeistvii;

    [Header("Карта и имя действия для самого джойстика (Vector2, без посторонних Interactions)")]
    public string imyaKartyDzhoystika = "XRI Left Interaction";
    public string imyaDeistviyaDzhoystika = "MenuJoystick";

    [Header("Карта и имя действия для клика стиком (кнопка)")]
    public string imyaKartyKlika = "XRI Left Interaction";
    public string imyaDeistviyaKlika = "MenuConfirm";

    [Header("Порог наклона, после которого засчитывается шаг")]
    public float porogNaklona = 0.5f;

    [Header("Диагностика: показывать сырые значения джойстика в консоли")]
    public bool pokazyvatSyrieZnacheniya = false;

    [Header("Шаг изменения слайдера за один наклон влево/вправо")]
    public float shagSlaidera = 0.1f;

    private InputAction deistvieDzhoystika;
    private InputAction deistvieKlika;

    private bool vnutriKategorii = false;
    private bool zhdyomNeytral = false;

    void Awake()
    {
        if (naborDeistvii == null)
        {
            Debug.LogWarning("DzhoystikNavigatsiyaMenu: naborDeistvii не назначен!");
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
            Debug.LogWarning("DzhoystikNavigatsiyaMenu: действие '" + imyaDeistviyaDzhoystika + "' не найдено в карте '" + imyaKartyDzhoystika + "'.");
        }
        else
        {
            Debug.Log("DzhoystikNavigatsiyaMenu: действие джойстика '" + imyaDeistviyaDzhoystika + "' найдено успешно.");
        }

        if (deistvieKlika == null)
        {
            Debug.LogWarning("DzhoystikNavigatsiyaMenu: действие '" + imyaDeistviyaKlika + "' не найдено в карте '" + imyaKartyKlika + "'.");
        }
        else
        {
            Debug.Log("DzhoystikNavigatsiyaMenu: действие клика '" + imyaDeistviyaKlika + "' найдено успешно.");
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

        // Сообщаем своё состояние. Реальное переключение локомоции делает
        // DzhoystikNavigatsiyaInventarya в своём LateUpdate.
        SostoyanieMenyu.ToolsMenuOpen = menyuOtkryto;

        if (!menyuOtkryto)
        {
            zhdyomNeytral = false;
            return;
        }

        // Если выделение сбросилось само по себе - восстанавливаем подсветку
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            VosstanovitPodsvetku();
        }
    }

    private void VosstanovitPodsvetku()
    {
        if (vnutriKategorii)
        {
            MenuVkladki tekushieVidy = PoluchitMenuVidovTekushiyeKategorii();
            if (tekushieVidy != null)
            {
                tekushieVidy.OtkrytVkladku(tekushieVidy.TekushayaVkladka);
            }
        }
        else if (menuKategoriy != null)
        {
            menuKategoriy.OtkrytVkladku(menuKategoriy.TekushayaVkladka);
        }
    }

    private void SobytieDzhoystika(InputAction.CallbackContext context)
    {
        bool menyuOtkryto = panelMenu != null && panelMenu.activeInHierarchy;

        Vector2 znachenie = context.ReadValue<Vector2>();

        if (pokazyvatSyrieZnacheniya)
        {
            Debug.Log("ToolsMenu: событие джойстика, значение = " + znachenie + " (меню открыто: " + menyuOtkryto + ")");
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
        bool eto_gorizontalniy_naklon = Mathf.Abs(napravlenie.x) > Mathf.Abs(napravlenie.y);

        if (eto_gorizontalniy_naklon)
        {
            ObrabotatGorizontalniyNaklon(napravlenie.x > 0);
        }
        else
        {
            if (napravlenie.y > 0)
            {
                PredydushiyElement();
            }
            else
            {
                SleduyuschiyElement();
            }
        }
    }

    // Что делает наклон влево/вправо - зависит от того, где мы сейчас
    private void ObrabotatGorizontalniyNaklon(bool vpravo)
    {
        if (vnutriKategorii)
        {
            MenuVkladki tekushieVidy = PoluchitMenuVidovTekushiyeKategorii();

            if (tekushieVidy != null)
            {
                // 1. Выбран слайдер - двигаем его значение
                if (tekushieVidy.TekushiyElementEstSlider())
                {
                    tekushieVidy.IzmenitSlaiderEsliVybran(vpravo ? shagSlaidera : -shagSlaidera);
                    return;
                }

                // 2. Вправо внутри категории: в сетке - шаг вправо, в списке - ничего
                if (vpravo)
                {
                    tekushieVidy.ShagVpravo();
                    return;
                }

                // 3. Влево: в сетке - шаг влево; если сдвинуться некуда (левая колонка
                //    или обычный список) - выходим из категории
                bool sdvinulis = tekushieVidy.ShagVlevo();
                if (!sdvinulis)
                {
                    VyytiIzKategorii();
                }

                return;
            }
        }

        if (vpravo)
        {
            VoytiVKategoriyu();
        }
        else
        {
            VyytiIzKategorii();
        }
    }

    private void VoytiVKategoriyu()
    {
        if (!vnutriKategorii && menuKategoriy != null)
        {
            vnutriKategorii = true;

            MenuVkladki tekushieVidy = PoluchitMenuVidovTekushiyeKategorii();
            if (tekushieVidy != null)
            {
                tekushieVidy.OtkrytVkladku(tekushieVidy.TekushayaVkladka);
            }
        }
    }

    private void VyytiIzKategorii()
    {
        if (vnutriKategorii)
        {
            vnutriKategorii = false;

            if (menuKategoriy != null)
            {
                menuKategoriy.OtkrytVkladku(menuKategoriy.TekushayaVkladka);
            }
        }
    }

    private void SleduyuschiyElement()
    {
        if (vnutriKategorii)
        {
            MenuVkladki tekushieVidy = PoluchitMenuVidovTekushiyeKategorii();
            if (tekushieVidy != null)
            {
                tekushieVidy.ShagVniz();
            }
        }
        else if (menuKategoriy != null)
        {
            menuKategoriy.SleduyuschayaVkladka();
        }
    }

    private void PredydushiyElement()
    {
        if (vnutriKategorii)
        {
            MenuVkladki tekushieVidy = PoluchitMenuVidovTekushiyeKategorii();
            if (tekushieVidy != null)
            {
                tekushieVidy.ShagVverkh();
            }
        }
        else if (menuKategoriy != null)
        {
            menuKategoriy.PredydushayaVkladka();
        }
    }

    private MenuVkladki PoluchitMenuVidovTekushiyeKategorii()
    {
        if (menuKategoriy == null || menuVidovPoKategoriyam == null)
        {
            return null;
        }

        int nomerKategorii = menuKategoriy.TekushayaVkladka;
        if (nomerKategorii < 0 || nomerKategorii >= menuVidovPoKategoriyam.Length)
        {
            return null;
        }

        return menuVidovPoKategoriyam[nomerKategorii];
    }

    private void NazhatieKlika(InputAction.CallbackContext context)
    {
        if (vnutriKategorii)
        {
            MenuVkladki tekushieVidy = PoluchitMenuVidovTekushiyeKategorii();
            if (tekushieVidy != null)
            {
                tekushieVidy.PodtverditViybor();
            }
        }
    }
}