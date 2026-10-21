using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ---- Данные каталога замены (настраиваются в инспекторе) ----

[Serializable]
public class VidZameny
{
    [Tooltip("Название вида, например 'Дуб'. Сравнивается с Nasazhdenie.vidNasazhdeniya для галочки")]
    public string nazvanie;

    public GameObject prefab;
}

[Serializable]
public class KategoriyaZameny
{
    [Tooltip("Точно как в Nasazhdenie.kategoriya: Клумбы / Газон / Деревья / Кусты")]
    public string nazvanie;

    [Tooltip("Не больше, чем ячеек в сетке (по умолчанию 4)")]
    public VidZameny[] vidy;
}

[Serializable]
public class YacheykaVida
{
    public Button knopka;
    public Text tekst;

    [Tooltip("Объект-галочка внутри кнопки, виден только у текущего вида")]
    public GameObject galochka;
}

[Flags]
public enum KrayVyhoda
{
    Nichego = 0,
    Verh = 1,
    Niz = 2,
    Levo = 4,
    Pravo = 8,
    Vse = 15
}

// Задачи 4.1 и 4.2. Панель выбора вида для замены.
// Показывает кнопки видов той категории, к которой относится выбранный объект,
// ставит галочку на текущем виде и даёт перемещаться по сетке 2x2 джойстиком.
// Свайп за край сетки закрывает панель.
//
// Вешать на саму панель PanelZameny. Панель открывается/закрывается через SetActive,
// подписка на ввод живёт в OnEnable/OnDisable, поэтому пока панель скрыта, она ввод не слушает.
// Саму замену панель не делает - она только сообщает событием VidVybran, какой вид выбрали.
public class PanelZameny : MonoBehaviour
{
    [Header("Каталог видов по категориям")]
    public KategoriyaZameny[] katalog;

    [Header("Ячейки сетки по порядку: слева направо, сверху вниз")]
    public YacheykaVida[] yacheyki;

    [Header("Сколько столбцов в сетке (для 2x2 - 2)")]
    public int kolonki = 2;

    [Header("Через какие края свайп закрывает панель")]
    public KrayVyhoda krayaVyhoda = KrayVyhoda.Vse;

    [Header("Весь asset с действиями")]
    public InputActionAsset naborDeistvii;

    [Header("Карта и имя действия для джойстика (Vector2) - как у панели инвентаря")]
    public string imyaKartyDzhoystika = "XRI Right Locomotion";
    public string imyaDeistviyaDzhoystika = "Turn";

    [Header("Карта и имя действия для клика стиком (кнопка)")]
    public string imyaKartyKlika = "XRI Right Interaction";
    public string imyaDeistviyaKlika = "MenuConfirm";

    [Header("Порог наклона, после которого засчитывается шаг")]
    public float porogNaklona = 0.5f;

    [Header("Порог возврата в нейтраль, после которого можно наклонять снова")]
    public float porogVozvrata = 0.3f;

    // Пользователь выбрал вид (клик стиком или нажатие кнопки лучом)
    public event Action<VidZameny> VidVybran;

    // Панель закрылась (любым способом: свайп, SetActive(false), закрытие родителя)
    public event Action Zakryta;

    private Nasazhdenie tekushiyObyekt;
    private VidZameny[] pokazannyeVidy = new VidZameny[0];
    private int tekushayaYacheyka = 0;

    private InputAction deistvieDzhoystika;
    private InputAction deistvieKlika;
    private bool zhdyomNeytral = false;
    private int kadrOtkrytiya = -1;
    private bool knopkiPodpisany = false;

    void Awake()
    {
        if (naborDeistvii == null)
        {
            Debug.LogWarning("PanelZameny: naborDeistvii не назначен!");
        }
        else
        {
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
                Debug.LogWarning("PanelZameny: действие '" + imyaDeistviyaDzhoystika + "' не найдено в карте '" + imyaKartyDzhoystika + "'.");
            }

            if (deistvieKlika == null)
            {
                Debug.LogWarning("PanelZameny: действие '" + imyaDeistviyaKlika + "' не найдено в карте '" + imyaKartyKlika + "'.");
            }
        }

        PodpisatKnopki();
    }

    void OnEnable()
    {
        kadrOtkrytiya = Time.frameCount;

        // Если стик уже отклонён в момент открытия - ждём, пока вернётся в нейтраль,
        // иначе первый же шаг сработает сам собой
        zhdyomNeytral = deistvieDzhoystika != null && deistvieDzhoystika.ReadValue<Vector2>().magnitude > porogVozvrata;

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

        PokazatVidy(true);
    }

    void OnDisable()
    {
        // Действия НЕ выключаем (Disable) - они общие с панелью инвентаря и другими скриптами,
        // только отписываемся от своих обработчиков
        if (deistvieDzhoystika != null)
        {
            deistvieDzhoystika.performed -= SobytieDzhoystika;
            deistvieDzhoystika.canceled -= SobytieDzhoystikaSbros;
        }

        if (deistvieKlika != null)
        {
            deistvieKlika.performed -= NazhatieKlika;
        }

        if (Zakryta != null)
        {
            Zakryta.Invoke();
        }
    }

    void Update()
    {
        // Если модуль ввода сбросил выделение - возвращаем его на текущую ячейку
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            VybratYacheyku(tekushayaYacheyka);
        }
    }

    // ---- Публичный интерфейс ----

    // Открывает панель для указанного объекта
    public void Otkryt(Nasazhdenie obyekt)
    {
        tekushiyObyekt = obyekt;
        gameObject.SetActive(true);
    }

    public void Zakryt()
    {
        gameObject.SetActive(false);
    }

    // Меняет объект, для которого открыта панель (например, после замены), и обновляет галочку
    public void UstanovitObyekt(Nasazhdenie obyekt)
    {
        tekushiyObyekt = obyekt;
        if (isActiveAndEnabled)
        {
            PokazatVidy(false);
        }
    }

    // Вернулся ли стик в нейтраль (нужно, чтобы после выхода свайпом не "прилипло" лишнее действие)
    public bool DzhoystikVNeytrali()
    {
        return deistvieDzhoystika == null || deistvieDzhoystika.ReadValue<Vector2>().magnitude < porogVozvrata;
    }

    // ---- 4.1. Кнопки видов и галочка ----

    private void PodpisatKnopki()
    {
        if (knopkiPodpisany || yacheyki == null)
        {
            return;
        }

        for (int i = 0; i < yacheyki.Length; i++)
        {
            if (yacheyki[i] == null || yacheyki[i].knopka == null)
            {
                continue;
            }

            int nomerYacheyki = i; // локальная копия, чтобы каждая кнопка запомнила свой номер
            yacheyki[i].knopka.onClick.AddListener(() => NazhatieYacheyki(nomerYacheyki));
        }

        knopkiPodpisany = true;
    }

    // Заполняет ячейки видами категории текущего объекта и расставляет галочку
    private void PokazatVidy(bool vybratTekushiyVid)
    {
        KategoriyaZameny kategoriya = NaytiKategoriyu(tekushiyObyekt != null ? tekushiyObyekt.kategoriya : null);

        if (kategoriya == null || kategoriya.vidy == null)
        {
            pokazannyeVidy = new VidZameny[0];
            if (tekushiyObyekt != null)
            {
                Debug.LogWarning("PanelZameny: в каталоге нет категории '" + tekushiyObyekt.kategoriya + "'.");
            }
        }
        else
        {
            pokazannyeVidy = kategoriya.vidy;

            if (pokazannyeVidy.Length > yacheyki.Length)
            {
                Debug.LogWarning("PanelZameny: видов в категории '" + kategoriya.nazvanie + "' больше, чем ячеек (" + yacheyki.Length + "). Лишние не показаны.");
            }
        }

        int indeksTekushegoVida = -1;

        for (int i = 0; i < yacheyki.Length; i++)
        {
            YacheykaVida yacheyka = yacheyki[i];
            if (yacheyka == null || yacheyka.knopka == null)
            {
                continue;
            }

            bool est_vid = YacheykaDostupna(i);
            yacheyka.knopka.gameObject.SetActive(est_vid);

            if (!est_vid)
            {
                continue;
            }

            VidZameny vid = pokazannyeVidy[i];

            if (yacheyka.tekst != null)
            {
                yacheyka.tekst.text = vid.nazvanie;
            }

            bool eto_tekushiy_vid = EtoTekushiyVid(vid);
            if (yacheyka.galochka != null)
            {
                yacheyka.galochka.SetActive(eto_tekushiy_vid);
            }

            if (eto_tekushiy_vid)
            {
                indeksTekushegoVida = i;
            }
        }

        if (vybratTekushiyVid)
        {
            // При открытии ставим выделение на текущий вид, а если его нет - на первый доступный
            int start = indeksTekushegoVida >= 0 ? indeksTekushegoVida : NaytiPervuyuDostupnuyu();
            VybratYacheyku(start);
        }
        else if (!YacheykaDostupna(tekushayaYacheyka))
        {
            VybratYacheyku(NaytiPervuyuDostupnuyu());
        }
    }

    private KategoriyaZameny NaytiKategoriyu(string nazvanie)
    {
        if (katalog == null || string.IsNullOrEmpty(nazvanie))
        {
            return null;
        }

        for (int i = 0; i < katalog.Length; i++)
        {
            if (katalog[i] != null && string.Equals(katalog[i].nazvanie, nazvanie, StringComparison.OrdinalIgnoreCase))
            {
                return katalog[i];
            }
        }

        return null;
    }

    private bool EtoTekushiyVid(VidZameny vid)
    {
        return tekushiyObyekt != null
            && vid != null
            && string.Equals(vid.nazvanie, tekushiyObyekt.vidNasazhdeniya, StringComparison.OrdinalIgnoreCase);
    }

    private bool YacheykaDostupna(int indeks)
    {
        return indeks >= 0
            && indeks < yacheyki.Length
            && indeks < pokazannyeVidy.Length
            && yacheyki[indeks] != null
            && yacheyki[indeks].knopka != null
            && pokazannyeVidy[indeks] != null;
    }

    private int NaytiPervuyuDostupnuyu()
    {
        for (int i = 0; i < yacheyki.Length; i++)
        {
            if (YacheykaDostupna(i))
            {
                return i;
            }
        }

        return 0;
    }

    private void VybratYacheyku(int indeks)
    {
        tekushayaYacheyka = indeks;

        if (EventSystem.current != null && YacheykaDostupna(indeks))
        {
            EventSystem.current.SetSelectedGameObject(yacheyki[indeks].knopka.gameObject);
        }
    }

    // Подтверждение выбора: и клик стиком, и нажатие кнопки лучом приходят сюда
    private void NazhatieYacheyki(int indeks)
    {
        if (!YacheykaDostupna(indeks))
        {
            return;
        }

        tekushayaYacheyka = indeks;

        if (VidVybran != null)
        {
            VidVybran.Invoke(pokazannyeVidy[indeks]);
        }
    }

    // ---- 4.2. Навигация по сетке джойстиком ----

    // Вызывается Input System'ой в момент появления значения (как и в панели инвентаря)
    private void SobytieDzhoystika(InputAction.CallbackContext context)
    {
        Vector2 znachenie = context.ReadValue<Vector2>();
        float velichina = znachenie.magnitude;

        if (velichina < porogVozvrata)
        {
            zhdyomNeytral = false;
            return;
        }

        if (!zhdyomNeytral && velichina > porogNaklona)
        {
            zhdyomNeytral = true;
            ObrabotatNaklon(znachenie);
        }
    }

    private void SobytieDzhoystikaSbros(InputAction.CallbackContext context)
    {
        zhdyomNeytral = false;
    }

    private void ObrabotatNaklon(Vector2 napravlenie)
    {
        int stolbcov = Mathf.Max(1, kolonki);
        int ryad = tekushayaYacheyka / stolbcov;
        int stolbets = tekushayaYacheyka % stolbcov;

        int sdvigRyada = 0;
        int sdvigStolbtsa = 0;
        KrayVyhoda kray;

        bool eto_gorizontalniy_naklon = Mathf.Abs(napravlenie.x) > Mathf.Abs(napravlenie.y);

        if (eto_gorizontalniy_naklon)
        {
            sdvigStolbtsa = napravlenie.x > 0 ? 1 : -1;
            kray = napravlenie.x > 0 ? KrayVyhoda.Pravo : KrayVyhoda.Levo;
        }
        else
        {
            sdvigRyada = napravlenie.y > 0 ? -1 : 1; // стик вверх - строка выше
            kray = napravlenie.y > 0 ? KrayVyhoda.Verh : KrayVyhoda.Niz;
        }

        int novyiRyad = ryad + sdvigRyada;
        int novyiStolbets = stolbets + sdvigStolbtsa;

        bool vnutriSetki = novyiRyad >= 0 && novyiStolbets >= 0 && novyiStolbets < stolbcov;
        int novayaYacheyka = novyiRyad * stolbcov + novyiStolbets;

        if (vnutriSetki && YacheykaDostupna(novayaYacheyka))
        {
            VybratYacheyku(novayaYacheyka);
            return;
        }

        // Дальше в этом направлении ячеек нет - это край сетки.
        // Если через этот край разрешён выход - закрываем панель.
        if ((krayaVyhoda & kray) != 0)
        {
            Zakryt();
        }
    }

    private void NazhatieKlika(InputAction.CallbackContext context)
    {
        // Тот же клик стика мог открыть эту панель (через кнопку "Заменить вид") - его игнорируем
        if (Time.frameCount == kadrOtkrytiya)
        {
            return;
        }

        NazhatieYacheyki(tekushayaYacheyka);
    }
}
