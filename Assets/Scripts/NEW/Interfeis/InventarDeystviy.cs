using UnityEngine;
using UnityEngine.UI;

public class InventarDeystviy : MonoBehaviour
{
    public Nasazhdenie tekushiyObyekt;
    public Text tekstNazvaniyaObyekta;
    public Button knopkaPovernut, knopkaRastyanut, knopkaZafiksirovat, knopkaUdalit, knopkaZamenitVid;

    [Header("Кнопка смены цвета (только для клумб)")]
    public Button knopkaSmenitTsvet;

    public Text tekstKnopkiZafiksirovat;
    public GameObject panelRazmer;
    public Slider slaiderRazmer;
    public float maksimalnoeIzmenenieRazmera = 3f;
    public Text tekstKnopkiPovernut, tekstKnopkiRastyanut;
    public VyborNasazhdeniya vybor;
    public PanelPereklyuchatel pereklyuchatelInventarya;
    public DzhoystikNavigatsiyaInventarya navigator;

    private bool rezhimVrascheniya = false;
    private bool rezhimRastyagivaniya = false;

    public bool RezhimVrascheniya => rezhimVrascheniya;
    public bool RezhimRastyagivaniya => rezhimRastyagivaniya;

    void Start()
    {
        knopkaPovernut.onClick.AddListener(Povernut);
        knopkaRastyanut.onClick.AddListener(Rastyanut);
        knopkaZafiksirovat.onClick.AddListener(PereklyuchitFiksatsiyu);
        knopkaUdalit.onClick.AddListener(Udalit);
        knopkaZamenitVid.onClick.AddListener(ZamenitVid);

        if (knopkaSmenitTsvet != null) knopkaSmenitTsvet.onClick.AddListener(SmenitTsvet);

        slaiderRazmer.minValue = 0f; slaiderRazmer.maxValue = 1f;
        slaiderRazmer.SetValueWithoutNotify(0.5f);
        slaiderRazmer.onValueChanged.AddListener(IzmenitRazmer);

        UstanovitTekushiyObyekt(tekushiyObyekt);
    }

    void OnDisable() { if (vybor != null) vybor.SbrositVydelenie(); }

    public void UstanovitTekushiyObyekt(Nasazhdenie novyiObyekt)
    {
        tekushiyObyekt = novyiObyekt;
        ZakonchitRezhimy();

        bool chto_to_vybrano = (tekushiyObyekt != null);

        // Базовые кнопки
        knopkaPovernut.interactable = chto_to_vybrano;
        knopkaZafiksirovat.interactable = chto_to_vybrano;
        knopkaUdalit.interactable = chto_to_vybrano;
        knopkaZamenitVid.interactable = chto_to_vybrano;
        panelRazmer.SetActive(chto_to_vybrano);

        if (chto_to_vybrano)
        {
            tekstNazvaniyaObyekta.text = tekushiyObyekt.vidNasazhdeniya;

            // Обновляем слайдер размера
            if (tekushiyObyekt.ishodnyiRazmer.x > 0.0001f && maksimalnoeIzmenenieRazmera > 1.0001f)
            {
                float mnozhitel = tekushiyObyekt.transform.localScale.x / tekushiyObyekt.ishodnyiRazmer.x;
                if (mnozhitel > 0.0001f)
                    slaiderRazmer.SetValueWithoutNotify(Mathf.Clamp01(0.5f + Mathf.Log(mnozhitel, maksimalnoeIzmenenieRazmera) / 2f));
            }

            // Определяем категории
            bool eto_gazon = (tekushiyObyekt.kategoriya == "Газон");
            bool eto_klumba = (tekushiyObyekt.kategoriya == "Клумбы");
            bool eto_sekciya = (tekushiyObyekt.kategoriya == "Секции");

            // Показываем нужные кнопки
            knopkaRastyanut.gameObject.SetActive(eto_gazon || eto_klumba || eto_sekciya);

            // Замену вида прячем для газона, клумбы и секции
            knopkaZamenitVid.gameObject.SetActive(!eto_gazon && !eto_klumba && !eto_sekciya);

            // Кнопку смены цвета показываем ТОЛЬКО для клумбы
            if (knopkaSmenitTsvet != null)
                knopkaSmenitTsvet.gameObject.SetActive(eto_klumba);

            ObnovitTekstFiksatsii();
        }
        else
        {
            tekstNazvaniyaObyekta.text = "Ничего не выбрано";

            // Прячем все контекстные кнопки, если ничего не выбрано
            knopkaRastyanut.gameObject.SetActive(false);
            knopkaZamenitVid.gameObject.SetActive(true); // Возвращаем в исходное

            if (knopkaSmenitTsvet != null)
                knopkaSmenitTsvet.gameObject.SetActive(false);
        }
    }

    public void ZakonchitRezhimy()
    {
        rezhimVrascheniya = false;
        ObnovitTekstPovernut();

        if (rezhimRastyagivaniya)
        {
            rezhimRastyagivaniya = false;
            if (tekushiyObyekt != null)
            {
                if (tekushiyObyekt.TryGetComponent<GazonTile>(out var gt)) gt.ZakonchitRastyagivanie();
                if (tekushiyObyekt.TryGetComponent<SekciyaNasazhdeniy>(out var sn)) sn.ZakonchitRastyagivanie();
                if (tekushiyObyekt.TryGetComponent<KlumbaTile>(out var kt)) kt.ZakonchitRastyagivanie();
            }
        }
        ObnovitTekstRastyanut();
    }

    private void Povernut()
    {
        if (tekushiyObyekt == null) return;
        if (rezhimRastyagivaniya) { rezhimRastyagivaniya = false; ObnovitTekstRastyanut(); }
        rezhimVrascheniya = !rezhimVrascheniya;
        ObnovitTekstPovernut();
    }

    public void Vrashat(float gradusy)
    {
        if (tekushiyObyekt != null && rezhimVrascheniya) tekushiyObyekt.transform.Rotate(Vector3.up, gradusy, Space.World);
    }

    public void ZakonchitVrashenie() { rezhimVrascheniya = false; ObnovitTekstPovernut(); }
    private void ObnovitTekstPovernut() { if (tekstKnopkiPovernut != null) tekstKnopkiPovernut.text = rezhimVrascheniya ? "Поворот: стик" : "Повернуть"; }

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
        if (tekushiyObyekt == null) return;

        bool isGazon = tekushiyObyekt.TryGetComponent<GazonTile>(out var gt);
        bool isSekciya = tekushiyObyekt.TryGetComponent<SekciyaNasazhdeniy>(out var sn);
        bool isKlumba = tekushiyObyekt.TryGetComponent<KlumbaTile>(out var kt);

        if (!isGazon && !isSekciya && !isKlumba) return;

        if (rezhimVrascheniya) { rezhimVrascheniya = false; ObnovitTekstPovernut(); }
        rezhimRastyagivaniya = !rezhimRastyagivaniya;

        if (!rezhimRastyagivaniya)
        {
            if (isGazon && gt != null) gt.ZakonchitRastyagivanie();
            if (isSekciya && sn != null) sn.ZakonchitRastyagivanie();
            if (isKlumba && kt != null) kt.ZakonchitRastyagivanie();
        }
        ObnovitTekstRastyanut();
    }

    private void ObnovitTekstRastyanut() { if (tekstKnopkiRastyanut != null) tekstKnopkiRastyanut.text = rezhimRastyagivaniya ? "Растягивание: Grip" : "Растянуть"; }

    private void SmenitTsvet()
    {
        if (tekushiyObyekt != null && tekushiyObyekt.TryGetComponent<KlumbaTile>(out var kt))
        {
            kt.SmenitTsvet();
        }
    }

    private void PereklyuchitFiksatsiyu()
    {
        if (tekushiyObyekt == null) return;
        if (vybor != null) vybor.SbrositVydelenie();
        if (pereklyuchatelInventarya != null) pereklyuchatelInventarya.Zakryt();
    }

    private void ObnovitTekstFiksatsii() { if (tekstKnopkiZafiksirovat != null && tekushiyObyekt != null) tekstKnopkiZafiksirovat.text = tekushiyObyekt.zafiksirovano ? "Открепить" : "Зафиксировать"; }
    private void Udalit() { if (tekushiyObyekt != null) { GameObject obj = tekushiyObyekt.gameObject; UstanovitTekushiyObyekt(null); Destroy(obj); } }
    private void ZamenitVid() { if (tekushiyObyekt != null && navigator != null) navigator.OtkrytPanelZameny(tekushiyObyekt); }
}