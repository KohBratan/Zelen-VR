using UnityEngine;
using UnityEngine.InputSystem;

// ѕока меню открыто, левый джойстик перестаЄт двигать персонажа
// и начинает управл€ть списком категорий и списком видов внутри категории.
//
// ¬верх/вниз - листание текущего списка
// ¬право     - зайти внутрь выбранной категории
// ¬лево      - выйти обратно к списку категорий
//  лик стика - подтвердить выбранный вид
public class DzhoystikNavigatsiyaMenu : MonoBehaviour
{
    [Header("ѕанель меню целиком (используетс€, чтобы пон€ть, открыто ли меню)")]
    public GameObject panelMenu;

    [Header("ћеню верхнего уровн€ Ч список категорий")]
    public MenuVkladki menuKategoriy;

    [Header("ћеню видов внутри каждой категории (тот же пор€док, что и категории)")]
    public MenuVkladki[] menuVidovPoKategoriyam;

    [Header(" омпонент передвижени€ персонажа, который нужно выключать, пока меню открыто")]
    public Behaviour komponentPeredvizheniya;

    [Header("¬есь asset с действи€ми")]
    public InputActionAsset naborDeistvii;

    [Header(" арта и им€ действи€ дл€ самого джойстика (Vector2)")]
    public string imyaKartyDzhoystika = "XRI Left Locomotion";
    public string imyaDeistviyaDzhoystika = "Move";

    [Header(" арта и им€ действи€ дл€ клика стиком (кнопка)")]
    public string imyaKartyKlika = "XRI Left Interaction";
    public string imyaDeistviyaKlika = "MenuConfirm";

    [Header("ѕорог наклона, после которого засчитываетс€ шаг")]
    public float porogNaklona = 0.5f;

    [Header("ѕорог возврата в нейтраль, после которого можно наклон€ть снова")]
    public float porogVozvrata = 0.3f;

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

        if (deistvieKlika == null)
        {
            Debug.LogWarning("DzhoystikNavigatsiyaMenu: действие '" + imyaDeistviyaKlika + "' не найдено в карте '" + imyaKartyKlika + "'.");
        }
    }

    void OnEnable()
    {
        if (deistvieDzhoystika != null)
        {
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
        if (deistvieKlika != null)
        {
            deistvieKlika.performed -= NazhatieKlika;
        }
    }

    void Update()
    {
        bool menyuOtkryto = panelMenu != null && panelMenu.activeInHierarchy;

        // ѕока меню открыто - выключаем обычное передвижение
        if (komponentPeredvizheniya != null)
        {
            komponentPeredvizheniya.enabled = !menyuOtkryto;
        }

        if (!menyuOtkryto || deistvieDzhoystika == null)
        {
            zhdyomNeytral = false;
            return;
        }

        Vector2 znachenie = deistvieDzhoystika.ReadValue<Vector2>();
        float velichina = znachenie.magnitude;

        if (!zhdyomNeytral && velichina > porogNaklona)
        {
            ObrabotatNaklon(znachenie);
            zhdyomNeytral = true;
        }
        else if (zhdyomNeytral && velichina < porogVozvrata)
        {
            zhdyomNeytral = false;
        }
    }

    private void ObrabotatNaklon(Vector2 napravlenie)
    {
        bool eto_gorizontalniy_naklon = Mathf.Abs(napravlenie.x) > Mathf.Abs(napravlenie.y);

        if (eto_gorizontalniy_naklon)
        {
            if (napravlenie.x > 0)
            {
                VoytiVKategoriyu();
            }
            else
            {
                VyytiIzKategorii();
            }
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

    private void VoytiVKategoriyu()
    {
        if (!vnutriKategorii && menuKategoriy != null)
        {
            vnutriKategorii = true;
        }
    }

    private void VyytiIzKategorii()
    {
        if (vnutriKategorii)
        {
            vnutriKategorii = false;
        }
    }

    private void SleduyuschiyElement()
    {
        if (vnutriKategorii)
        {
            MenuVkladki tekushieVidy = PoluchitMenuVidovTekushiyeKategorii();
            if (tekushieVidy != null)
            {
                tekushieVidy.SleduyuschayaVkladka();
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
                tekushieVidy.PredydushayaVkladka();
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