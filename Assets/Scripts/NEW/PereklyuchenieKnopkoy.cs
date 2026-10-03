using UnityEngine;
using UnityEngine.InputSystem;

// ѕо нажатию кнопки на контроллере (Y слева или A справа) переключает
// вкладку/слот в указанном MenuVkladki на следующий по кругу.
public class PereklyuchenieKnopkoy : MonoBehaviour
{
    [Header("ћеню, вкладки которого переключаем")]
    public MenuVkladki menu;

    [Header("¬есь asset с действи€ми (например, XRI Default Input Actions)")]
    public InputActionAsset naborDeistvii;

    [Header("»м€ карты действий, как оно написано в asset'е")]
    public string imyaKarty = "XRI Left Interaction";

    [Header("»м€ действи€ переключени€, как оно написано в asset'е")]
    public string imyaDeistviya = "CycleToolsMenu";

    private InputAction deistviePereklyucheniya;

    void Awake()
    {
        if (naborDeistvii == null)
        {
            Debug.LogWarning("PereklyuchenieKnopkoy: naborDeistvii не назначен в инспекторе!");
            return;
        }

        var karta = naborDeistvii.FindActionMap(imyaKarty);
        if (karta == null)
        {
            Debug.LogWarning("PereklyuchenieKnopkoy: карта действий с именем '" + imyaKarty + "' не найдена.");
            return;
        }

        deistviePereklyucheniya = karta.FindAction(imyaDeistviya);
        if (deistviePereklyucheniya == null)
        {
            Debug.LogWarning("PereklyuchenieKnopkoy: действие с именем '" + imyaDeistviya + "' не найдено в карте '" + imyaKarty + "'.");
        }
    }

    void OnEnable()
    {
        if (deistviePereklyucheniya != null)
        {
            deistviePereklyucheniya.performed += NazhatieKnopki;
            deistviePereklyucheniya.Enable();
        }
    }

    void OnDisable()
    {
        if (deistviePereklyucheniya != null)
        {
            deistviePereklyucheniya.performed -= NazhatieKnopki;
        }
    }

    private void NazhatieKnopki(InputAction.CallbackContext context)
    {
        if (menu != null)
        {
            menu.SleduyuschayaVkladka();
        }
    }
}