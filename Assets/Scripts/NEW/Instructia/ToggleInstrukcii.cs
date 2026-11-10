// ---------------------------------------------------------------------------
// Zelen-VR
// File: ToggleInstrukcii.cs
// Pereklyuchenie instruktsii.
// ---------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleInstrukcii : MonoBehaviour
{
    [Header("Сама панель инструкции (Canvas)")]
    public InstrukciyaPanel panel;

    [Header("Весь asset с действиями")]
    public InputActionAsset naborDeistvii;

    [Header("Карта и действие для кнопки 'Меню' (Button)")]
    public string imyaKarty = "XRI Left Interaction";
    public string imyaDeistviya = "ToggleHelp";

    private InputAction deistvie;

    void Awake()
    {
        if (naborDeistvii == null) return;
        var karta = naborDeistvii.FindActionMap(imyaKarty);
        if (karta != null) deistvie = karta.FindAction(imyaDeistviya);
    }

    void OnEnable()
    {
        if (deistvie != null)
        {
            deistvie.performed += Nazhatie;
            deistvie.Enable();
        }
    }

    void OnDisable()
    {
        if (deistvie != null)
        {
            deistvie.performed -= Nazhatie;
        }
    }

    private void Nazhatie(InputAction.CallbackContext context)
    {
        if (panel == null) return;

        // Если панель выключена - включаем и ставим перед игроком
        if (!panel.gameObject.activeSelf)
        {
            panel.gameObject.SetActive(true);
            panel.PostavitPeredIgrokom();
        }
        // Если включена - просто выключаем
        else
        {
            panel.gameObject.SetActive(false);
        }
    }
}