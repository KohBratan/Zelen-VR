using UnityEngine;
using UnityEngine.UI;

// Вешается на каждую кнопку-картинку в списке (дерево, куст, газон и т.д.).
// Клик по кнопке выбирает префаб. Повторный клик по уже выбранной кнопке
// снимает выбор (инструмент убирается из руки).
public class KnopkaNasazhdeniya : MonoBehaviour
{
    [Header("Префаб, который будет ставиться на сцену, когда выбрана эта кнопка")]
    public GameObject prefabNasazhdeniya;

    [Header("Метка 'выбрано' (галочка или рамка), необязательно")]
    public GameObject metkaVybrano;

    [Header("Забывать выбор, когда панель скрывается (смена категории или закрытие меню)")]
    public bool sbrasyvatPriSkrytii = true;

    private Button knopka;

    void Awake()
    {
        knopka = GetComponent<Button>();
    }

    void Start()
    {
        if (knopka != null)
        {
            knopka.onClick.AddListener(Vybrat);
        }

        ObnovitMetku();
    }

    void Update()
    {
        ObnovitMetku();
    }

    void OnDisable()
    {
        bool eto_vybrannoe = prefabNasazhdeniya != null && VyborInstrumenta.TekushiyPrefab == prefabNasazhdeniya;

        if (sbrasyvatPriSkrytii && eto_vybrannoe)
        {
            VyborInstrumenta.TekushiyPrefab = null;
            Debug.Log("Выбор сброшен: " + prefabNasazhdeniya.name);
        }
    }

    private void Vybrat()
    {
        if (prefabNasazhdeniya == null)
        {
            Debug.LogWarning("У кнопки '" + gameObject.name + "' не назначен prefabNasazhdeniya!");
            return;
        }

        if (VyborInstrumenta.TekushiyPrefab == prefabNasazhdeniya)
        {
            // Уже выбрано - снимаем выбор
            VyborInstrumenta.TekushiyPrefab = null;
            Debug.Log("Выбор сброшен (повторный клик): " + prefabNasazhdeniya.name);
        }
        else
        {
            // Не выбрано - выбираем
            VyborInstrumenta.TekushiyPrefab = prefabNasazhdeniya;
            Debug.Log("Выбрано насаждение: " + prefabNasazhdeniya.name);
            ZakrytMenyuInstrumentov();
        }
    }

    // После выбора прячем меню инструментов, чтобы сразу можно было ставить.
    private void ZakrytMenyuInstrumentov()
    {
        PanelPereklyuchatel[] vse = FindObjectsOfType<PanelPereklyuchatel>(true);
        for (int i = 0; i < vse.Length; i++)
        {
            PanelPereklyuchatel p = vse[i];
            if (p != null && p.panel != null && transform.IsChildOf(p.panel.transform))
            {
                p.Zakryt();
                return;
            }
        }
    }

    private void ObnovitMetku()
    {
        if (metkaVybrano != null)
        {
            bool eto_vybrannoe = prefabNasazhdeniya != null && VyborInstrumenta.TekushiyPrefab == prefabNasazhdeniya;
            metkaVybrano.SetActive(eto_vybrannoe);
        }
    }
}