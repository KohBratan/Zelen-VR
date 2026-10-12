using UnityEngine;
using UnityEngine.UI;

// Вешается на каждую кнопку-картинку в панели "Заменить вид".
// У кнопки должен быть дочерний объект-галочка (MetkaVybrano), который
// показывает, какой вид у текущего дерева.
public class KnopkaZamenyVida : MonoBehaviour
{
    [Header("Префаб этого вида (тот же, что у кнопки в меню инструментов)")]
    public GameObject prefabNasazhdeniya;

    [Header("Метка 'текущий вид' (галочка или рамка), необязательно")]
    public GameObject metkaVybrano;

    [Header("Навигатор инвентаря - он применит замену к дереву")]
    public DzhoystikNavigatsiyaInventarya navigator;

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
    }

    // Показать/спрятать галочку "текущий вид"
    public void UstanovitMetku(bool vklyucheno)
    {
        if (metkaVybrano != null)
        {
            metkaVybrano.SetActive(vklyucheno);
        }
    }

    // Нажать на кнопку из кода (используется навигатором)
    public void Nazhat()
    {
        if (knopka != null && knopka.interactable)
        {
            knopka.onClick.Invoke();
        }
    }

    private void Vybrat()
    {
        if (navigator != null)
        {
            navigator.PrimenitZamenu(this);
        }
        else
        {
            Debug.LogWarning("KnopkaZamenyVida: navigator не назначен на '" + gameObject.name + "'.");
        }
    }
}