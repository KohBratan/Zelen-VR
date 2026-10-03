using UnityEngine;
using UnityEngine.UI;

// Панель контекстных действий над выбранным объектом.
// Пока без логики хватания — tekushiyObyekt можно назначить вручную в инспекторе
// для теста, позже сюда будет приходить объект из скрипта захвата.
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

    void Start()
    {
        knopkaPovernut.onClick.AddListener(Povernut);
        knopkaRastyanut.onClick.AddListener(Rastyanut);
        knopkaZafiksirovat.onClick.AddListener(PereklyuchitFiksatsiyu);
        knopkaUdalit.onClick.AddListener(Udalit);
        knopkaZamenitVid.onClick.AddListener(ZamenitVid);

        slaiderRazmer.onValueChanged.AddListener(IzmenitRazmer);

        UstanovitTekushiyObyekt(tekushiyObyekt);
    }

    // Вызывается снаружи (позже — из скрипта захвата), когда меняется выбранный объект
    public void UstanovitTekushiyObyekt(Nasazhdenie novyiObyekt)
    {
        tekushiyObyekt = novyiObyekt;

        bool chto_to_vybrano = (tekushiyObyekt != null);

        knopkaPovernut.interactable = chto_to_vybrano;
        knopkaZafiksirovat.interactable = chto_to_vybrano;
        knopkaUdalit.interactable = chto_to_vybrano;
        knopkaZamenitVid.interactable = chto_to_vybrano;
        panelRazmer.SetActive(chto_to_vybrano);

        if (chto_to_vybrano)
        {
            tekstNazvaniyaObyekta.text = tekushiyObyekt.vidNasazhdeniya;

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

    private void Povernut()
    {
        if (tekushiyObyekt != null)
        {
            tekushiyObyekt.transform.Rotate(Vector3.up, 45f);
        }
    }

    private void IzmenitRazmer(float znachenie)
    {
        if (tekushiyObyekt != null)
        {
            tekushiyObyekt.transform.localScale = Vector3.one * znachenie;
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

    private void PereklyuchitFiksatsiyu()
    {
        if (tekushiyObyekt != null)
        {
            tekushiyObyekt.zafiksirovano = !tekushiyObyekt.zafiksirovano;
            ObnovitTekstFiksatsii();
        }
    }

    private void ObnovitTekstFiksatsii()
    {
        if (tekstKnopkiZafiksirovat != null && tekushiyObyekt != null)
        {
            tekstKnopkiZafiksirovat.text = tekushiyObyekt.zafiksirovano ? "Открепить" : "Зафиксировать";
        }
    }

    private void Udalit()
    {
        if (tekushiyObyekt != null)
        {
            GameObject obyektDlyaUdaleniya = tekushiyObyekt.gameObject;
            UstanovitTekushiyObyekt(null);
            Destroy(obyektDlyaUdaleniya);
        }
    }

    private void ZamenitVid()
    {
        Debug.Log("Замена вида пока не реализована — здесь позже откроется каталог насаждений.");
    }
}