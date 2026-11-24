using UnityEngine;
using UnityEngine.UI;

public class PanelSekcii : MonoBehaviour
{
    [System.Serializable]
    public class KnopkaSekcii
    {
        public Button knopka;
        public GameObject prefab;
        public string tip;
        [HideInInspector] public bool aktívna = false;
    }

    [Header("Массив из 8 кнопок")]
    public KnopkaSekcii[] knopki;

    [Header("Кнопка 'Поставить'")]
    public Button knopkaPostavit;
    public GameObject prefabUniversalnoySekcii;

    [Header("Слайдер плотности (вертикальный)")]
    public Slider slaiderPlotnosti;

    [Header("Ссылка на скрипт закрытия меню (PanelPereklyuchatel)")]
    public PanelPereklyuchatel pereklyuchatelMenyu; // <-- ВАЖНО: Перетащи сюда PanelPereklyuchatel с левой руки

    void Start()
    {
        foreach (var k in knopki)
        {
            if (k.knopka != null)
            {
                k.knopka.onClick.AddListener(() => PerekluchitKnopku(k));
            }
        }

        if (knopkaPostavit != null)
        {
            knopkaPostavit.onClick.AddListener(PostavitSekciyu);
        }

        if (slaiderPlotnosti != null)
        {
            slaiderPlotnosti.value = 0.5f;
            VyborSekcii.Plotnost = 0.5f;
            slaiderPlotnosti.onValueChanged.AddListener(IzmenitPlotnost);
        }
    }

    private void IzmenitPlotnost(float val)
    {
        VyborSekcii.Plotnost = val;
    }

    private void PerekluchitKnopku(KnopkaSekcii k)
    {
        k.aktívna = !k.aktívna;

        ColorBlock colors = k.knopka.colors;
        colors.normalColor = k.aktívna ? Color.green : Color.white;
        k.knopka.colors = colors;

        if (k.aktívna)
        {
            if (k.tip == "Дерево") VyborSekcii.Derevya.Add(k.prefab);
            if (k.tip == "Куст") VyborSekcii.Kusty.Add(k.prefab);
        }
        else
        {
            if (k.tip == "Дерево") VyborSekcii.Derevya.Remove(k.prefab);
            if (k.tip == "Куст") VyborSekcii.Kusty.Remove(k.prefab);
        }
    }

    private void PostavitSekciyu()
    {
        if (!VyborSekcii.EstVybor())
        {
            Debug.LogWarning("Выберите хотя бы одно растение!");
            return;
        }

        // Записываем префаб секции в глобальную переменную
        VyborInstrumenta.TekushiyPrefab = prefabUniversalnoySekcii;

        // ИСПРАВЛЕНИЕ: Принудительно закрываем меню, чтобы заработал луч секции
        if (pereklyuchatelMenyu != null)
        {
            pereklyuchatelMenyu.Zakryt();
        }
        else
        {
            Debug.LogError("PanelSekcii: Не назначен pereklyuchatelMenyu! Меню не закроется, луч не появится.");
        }
    }
}