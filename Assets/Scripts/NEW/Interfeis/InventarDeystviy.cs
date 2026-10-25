using UnityEngine;
using UnityEngine.UI;

// Панель контекстных действий над выбранным объектом.
// Содержит кнопки: Повернуть / Растянуть / Зафиксировать / Удалить / Заменить вид.
// Показ кнопок зависит от категории объекта (Растянуть - только для газона и клумб).
public class InventarDeystviy : MonoBehaviour
{
    [Header("Текущий выбранный объект (null, если ничего не выбрано)")]
    public Nasazhdenie tekushiyObyekt;

    [Header("Текст с названием выбранного объекта")]
    public Text tekstNazvaniyaObyekta;

    [Header("Кнопки действий")]
    public Button knopkaPovernut;
    public Button knopkaRastyanut;
    public Button knopkaZafiksirovat;
    public Button knopkaUdalit;
    public Button knopkaZamenitVid;

    [Header("Текст на кнопке фиксации")]
    public Text tekstKnopkiZafiksirovat;

    [Header("Панель изменения размера")]
    public GameObject panelRazmer;
    public Slider slaiderRazmer;

    [Header("Шаг растягивания за одно нажатие кнопки")]
    public float shagRastyagivaniya = 0.2f;

    [Header("Во сколько раз можно менять размер в каждую сторону (слайдер по центру = обычный размер)")]
    public float maksimalnoeIzmenenieRazmera = 3f;

    [Header("Текст на кнопке 'Повернуть' (необязательно) - показывает, что режим поворота включён")]
    public Text tekstKnopkiPovernut;

    [Header("Скрипт выбора насаждения (для сброса подсветки)")]
    public VyborNasazhdeniya vybor;

    [Header("Переключатель панели инвентаря (для закрытия по 'Зафиксировать')")]
    public PanelPereklyuchatel pereklyuchatelInventarya;

    [Header("Навигатор инвентаря (открывает панель замены вида)")]
    public DzhoystikNavigatsiyaInventarya navigator;

    private bool rezhimVrascheniya = false;

    public bool RezhimVrascheniya
    {
        get { return rezhimVrascheniya; }
    }

    void Start()
    {
        knopkaPovernut.onClick.AddListener(Povernut);
        knopkaRastyanut.onClick.AddListener(Rastyanut);
        knopkaZafiksirovat.onClick.AddListener(Zafiksirovat);
        knopkaUdalit.onClick.AddListener(Udalit);
        knopkaZamenitVid.onClick.AddListener(ZamenitVid);

        slaiderRazmer.minValue = 0f;
        slaiderRazmer.maxValue = 1f;
        slaiderRazmer.wholeNumbers = false;
        slaiderRazmer.SetValueWithoutNotify(0.5f);
        slaiderRazmer.onValueChanged.AddListener(IzmenitRazmer);

        UstanovitTekushiyObyekt(tekushiyObyekt);
    }

    // Панель выключается (закрытие инвентаря любым способом) -
    // сбрасываем выбор на объекте и очищаем себя.
    void OnDisable()
    {
        if (vybor != null)
        {
            vybor.SbrositVydelenie();
        }
    }

    public void UstanovitTekushiyObyekt(Nasazhdenie novyiObyekt)
    {
        tekushiyObyekt = novyiObyekt;
        ZakonchitVrashenie();

        bool chto_to_vybrano = (tekushiyObyekt != null);

        knopkaPovernut.interactable = chto_to_vybrano;
        knopkaZafiksirovat.interactable = chto_to_vybrano;
        knopkaUdalit.interactable = chto_to_vybrano;
        knopkaZamenitVid.interactable = chto_to_vybrano;
        panelRazmer.SetActive(chto_to_vybrano);

        if (chto_to_vybrano)
        {
            tekstNazvaniyaObyekta.text = tekushiyObyekt.vidNasazhdeniya;

            if (tekushiyObyekt.ishodnyiRazmer.x > 0.0001f && maksimalnoeIzmenenieRazmera > 1.0001f)
            {
                float mnozhitel = tekushiyObyekt.transform.localScale.x / tekushiyObyekt.ishodnyiRazmer.x;
                if (mnozhitel > 0.0001f)
                {
                    float polozhenie = 0.5f + Mathf.Log(mnozhitel, maksimalnoeIzmenenieRazmera) / 2f;
                    slaiderRazmer.SetValueWithoutNotify(Mathf.Clamp01(polozhenie));
                }
            }

            bool eto_gazon_ili_klumba = (tekushiyObyekt.kategoriya == "Газон" || tekushiyObyekt.kategoriya == "Клумбы");
            knopkaRastyanut.gameObject.SetActive(eto_gazon_ili_klumba);

            ObnovitTekstFiksatsii();
        }
        else
        {
            tekstNazvaniyaObyekta.text = "Ничего не выбрано";
            knopkaRastyanut.gameObject.SetActive(false);
        }
    }

    // ===== Действия над выбранным объектом =====

    private void Povernut()
    {
        if (tekushiyObyekt == null)
        {
            return;
        }

        rezhimVrascheniya = !rezhimVrascheniya;
        ObnovitTekstPovernut();
    }

    public void Vrashat(float gradusy)
    {
        if (tekushiyObyekt != null && rezhimVrascheniya)
        {
            tekushiyObyekt.transform.Rotate(Vector3.up, gradusy, Space.World);
        }
    }

    public void ZakonchitVrashenie()
    {
        rezhimVrascheniya = false;
        ObnovitTekstPovernut();
    }

    private void ObnovitTekstPovernut()
    {
        if (tekstKnopkiPovernut != null)
        {
            tekstKnopkiPovernut.text = rezhimVrascheniya ? "Поворот: стик" : "Повернуть";
        }
    }

    private void IzmenitRazmer(float znachenie)
    {
        if (tekushiyObyekt != null)
        {
            float mnozhitel = Mathf.Pow(maksimalnoeIzmenenieRazmera, (znachenie - 0.5f) * 2f);
            tekushiyObyekt.transform.localScale = tekushiyObyekt.ishodnyiRazmer * mnozhitel;
        }
    }

    private void Rastyanut()
    {
        if (tekushiyObyekt != null)
        {
            Vector3 tekushiyRazmer = tekushiyObyekt.transform.localScale;
            tekushiyObyekt.transform.localScale = new Vector3(tekushiyRazmer.x, tekushiyRazmer.y, tekushiyRazmer.z + shagRastyagivaniya);
        }
    }

    // ===== Удаление объекта =====

    // Удаляет текущий выбранный объект со сцены и очищает панель инвентаря.
    // Позиция и состояние инвентаря после удаления - "Ничего не выбрано".
    private void Udalit()
    {
        if (tekushiyObyekt == null)
        {
            return;
        }

        GameObject obyektDlyaUdaleniya = tekushiyObyekt.gameObject;

        // Сначала обнуляем инвентарь, чтобы не осталось висячих ссылок
        UstanovitTekushiyObyekt(null);

        Destroy(obyektDlyaUdaleniya);
        Debug.Log("Объект удалён: " + obyektDlyaUdaleniya.name);
    }

    // ===== "Зафиксировать" =====

    // Завершает работу с объектом: снимает подсветку, забывает выбор
    // и закрывает панель инвентаря. Следующий выбор - только через триггер,
    // игрок не остаётся "с прошлым" объектом.
    private void Zafiksirovat()
    {
        if (tekushiyObyekt == null)
        {
            return;
        }

        if (vybor != null)
        {
            vybor.SbrositVydelenie();
        }

        if (pereklyuchatelInventarya != null)
        {
            pereklyuchatelInventarya.Zakryt();
        }
        else
        {
            Debug.LogWarning("InventarDeystviy: pereklyuchatelInventarya не назначен - не могу закрыть инвентарь.");
        }

        Debug.Log("Зафиксировано и закрыто: " + tekushiyObyekt.vidNasazhdeniya);
    }

    private void ObnovitTekstFiksatsii()
    {
        if (tekstKnopkiZafiksirovat != null && tekushiyObyekt != null)
        {
            tekstKnopkiZafiksirovat.text = tekushiyObyekt.zafiksirovano ? "Открепить" : "Зафиксировать";
        }
    }

    // ===== "Заменить вид" =====

    private void ZamenitVid()
    {
        if (tekushiyObyekt == null)
        {
            return;
        }

        if (navigator == null)
        {
            Debug.LogWarning("InventarDeystviy: navigator не назначен - не могу открыть панель замены.");
            return;
        }

        navigator.OtkrytPanelZameny(tekushiyObyekt);
    }
}
