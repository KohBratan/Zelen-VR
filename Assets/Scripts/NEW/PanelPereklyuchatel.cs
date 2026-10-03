using UnityEngine;
using UnityEngine.InputSystem;

// Открывает/закрывает панель UI по нажатию кнопки на контроллере.
// ВАЖНО: вешать этот скрипт нужно на отдельный объект, который сам
// никогда не выключается (например, ToolsMenuManager или InventoryManager),
// а не на саму панель — иначе скрипт перестанет слышать нажатия кнопки,
// когда панель скрыта.
public class PanelPereklyuchatel : MonoBehaviour
{
    [Header("Панель, которую нужно показывать/прятать")]
    public GameObject panel;

    [Header("Весь asset с действиями (например, XRI Default Input Actions)")]
    public InputActionAsset naborDeistvii;

    [Header("Имя карты действий, как оно написано в asset'е")]
    public string imyaKarty = "XRI LeftHand Interaction";

    [Header("Имя самого действия, как оно написано в asset'е")]
    public string imyaDeistviya = "OpenToolsMenu";

    private InputAction deistvieOtkrytiya;
    private bool panelOtkryta = false;

    void Awake()
    {
        if (naborDeistvii == null)
        {
            Debug.LogWarning("PanelPereklyuchatel: naborDeistvii не назначен в инспекторе!");
            return;
        }

        var karta = naborDeistvii.FindActionMap(imyaKarty);
        if (karta == null)
        {
            Debug.LogWarning("PanelPereklyuchatel: карта действий с именем '" + imyaKarty + "' не найдена. Проверь точное написание в asset'е.");
            return;
        }

        deistvieOtkrytiya = karta.FindAction(imyaDeistviya);
        if (deistvieOtkrytiya == null)
        {
            Debug.LogWarning("PanelPereklyuchatel: действие с именем '" + imyaDeistviya + "' не найдено в карте '" + imyaKarty + "'. Проверь точное написание.");
        }
        else
        {
            Debug.Log("PanelPereklyuchatel: действие '" + imyaDeistviya + "' найдено успешно.");
        }
    }

    void Start()
    {
        panelOtkryta = false;
        if (panel != null)
        {
            panel.SetActive(panelOtkryta);
        }
    }

    void OnEnable()
    {
        if (deistvieOtkrytiya != null)
        {
            deistvieOtkrytiya.performed += NazhatieKnopki;
            deistvieOtkrytiya.Enable();
        }
    }

    void OnDisable()
    {
        if (deistvieOtkrytiya != null)
        {
            deistvieOtkrytiya.performed -= NazhatieKnopki;
        }
    }

    private void NazhatieKnopki(InputAction.CallbackContext context)
    {
        Debug.Log("PanelPereklyuchatel: нажатие кнопки '" + imyaDeistviya + "' сработало!");
        panelOtkryta = !panelOtkryta;
        if (panel != null)
        {
            panel.SetActive(panelOtkryta);
        }
    }
}