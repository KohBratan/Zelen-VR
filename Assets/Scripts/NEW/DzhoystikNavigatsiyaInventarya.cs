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
public class DzhoystikNavigatsiyaInventarya : MonoBehaviour
{
    [Header("Панель инвентаря целиком (чтобы понять, открыта ли она)")]
    public GameObject panelMenu;

    [Header("Элементы списка по порядку (кнопки и слайдеры)")]
    public Selectable[] elementyMenu;

    [Header("Шаг изменения слайдера за один наклон влево/вправо")]
    public float shagSlaidera = 0.1f;

    [Header("Компонент поворота камеры, который нужно выключать, пока инвентарь открыт")]
    public Behaviour komponentPovorota;

    [Header("Весь asset с действиями")]
    public InputActionAsset naborDeistvii;

    [Header("Карта и имя действия для самого джойстика (Vector2)")]
    public string imyaKartyDzhoystika = "XRI Right Locomotion";
    public string imyaDeistviyaDzhoystika = "Turn";

    [Header("Карта и имя действия для клика стиком (кнопка)")]
    public string imyaKartyKlika = "XRI Right Interaction";
    public string imyaDeistviyaKlika = "MenuConfirm";

    [Header("Порог наклона, после которого засчитывается шаг")]
    public float porogNaklona = 0.5f;

    [Header("Диагностика: показывать сырые значения джойстика в консоли")]
    public bool pokazyvatSyrieZnacheniya = false;

    private InputAction deistvieDzhoystika;
    private InputAction deistvieKlika;

    private int tekushiyIndeks = 0;
    private bool zhdyomNeytral = false;
    private bool panelByloOtkrytoProshlyiKadr = false;

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

        if (komponentPovorota != null)
        {
            komponentPovorota.enabled = !menyuOtkryto;
        }

        if (!menyuOtkryto)
        {
            zhdyomNeytral = false;
            panelByloOtkrytoProshlyiKadr = false;
            return;
        }

        // Панель только что открылась - выбираем первый доступный элемент
        if (!panelByloOtkrytoProshlyiKadr)
        {
            VybratElementPoIndeksu(NaytiPervyiDostupniyIndeks());
        }
        // Если выделение сбросилось само по себе (например, модуль ввода очистил его) -
        // восстанавливаем ТЕКУЩИЙ индекс, а не откатываемся на первый элемент
        else if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            VybratElementPoIndeksu(tekushiyIndeks);
        }

        panelByloOtkrytoProshlyiKadr = menyuOtkryto;
    }

    // Вызывается Input System'ой именно в тот момент, когда появляется значение
    // (важно для действий вроде Snap Turn, где значение живёт только один кадр)
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
}