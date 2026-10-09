using UnityEngine;
using UnityEngine.UI;

// Вешается на каждую кнопку-картинку в списке (дерево, куст и т.д.).
// Когда выбор подтверждён (клик стика), запоминает, какой префаб выбран,
// и показывает на этой кнопке метку "выбрано".
public class KnopkaNasazhdeniya : MonoBehaviour
{
    [Header("Префаб, который будет ставиться на сцену, когда выбрана эта кнопка")]
    public GameObject prefabNasazhdeniya;

    [Header("Метка 'выбрано' (галочка или рамка), необязательно")]
    public GameObject metkaVybrano;

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

    private void Vybrat()
    {
        VyborInstrumenta.TekushiyPrefab = prefabNasazhdeniya;

        if (prefabNasazhdeniya != null)
        {
            Debug.Log("Выбрано насаждение: " + prefabNasazhdeniya.name);
        }
        else
        {
            Debug.LogWarning("У кнопки '" + gameObject.name + "' не назначен prefabNasazhdeniya!");
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