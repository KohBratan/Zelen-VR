using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

// Игровое меню открывается системной кнопкой Menu любого контроллера.
// Скрипт висит на постоянно активном объекте, а не на скрываемой панели.
public class IgrovoeMenu : MonoBehaviour
{
    [Header("Панели")]
    public GameObject panelMenu;
    public GameObject panelInformacii;
    public GameObject panelKratkihPodskazok;

    [Header("VR-камера")]
    public Transform kamera;
    public float rasstoyanieDoMenyu = 2f;
    public float rasstoyanieDoPodskazok = 1.25f;

    [Header("Панели инструментов и инвентаря, закрываемые при открытии меню")]
    public PanelPereklyuchatel[] zakryvaemyePaneli;

    [Header("Кнопки")]
    public Button knopkaProdolzhit;
    public Button knopkaInformaciya;
    public Button knopkaPodskazki;
    public Button knopkaGlavnoeMenu;
    public Button knopkaVyyti;
    public Button knopkaNazadOtInformacii;

    [Header("Ввод")]
    public InputActionAsset naborDeistvii;
    public string imyaKarty = "XRI Left Interaction";
    public string imyaDeistviya = "OpenGameMenu";

    [Header("Переход в главное меню")]
    public string imyaScenyGlavnogoMenyu = "MainMenu";

    private InputAction deistvieMenyu;
    private readonly List<InputAction> zablokirovannyeDeistviya = new List<InputAction>();
    private readonly List<LuchMenyu> luchiMenyu = new List<LuchMenyu>();

    private sealed class LuchMenyu
    {
        public NearFarInteractor obychnyiInteractor;
        public XRRayInteractor interactorMenyu;
        public bool obychnyiBylVkljuchen;
    }

    void Awake()
    {
        if (naborDeistvii != null)
        {
            InputActionMap karta = naborDeistvii.FindActionMap(imyaKarty);
            if (karta != null)
            {
                deistvieMenyu = karta.FindAction(imyaDeistviya);
            }
        }

        if (deistvieMenyu == null)
        {
            Debug.LogWarning("IgrovoeMenu: действие '" + imyaDeistviya + "' не найдено.");
        }
    }

    void OnEnable()
    {
        if (deistvieMenyu != null)
        {
            deistvieMenyu.performed += NazhatieMenu;
            deistvieMenyu.Enable();
        }

        DobavitSlushateli();
    }

    void OnDisable()
    {
        if (deistvieMenyu != null)
        {
            deistvieMenyu.performed -= NazhatieMenu;
        }

        UdalitSlushateli();
        VyklyuchitLuchiMenyu();
        VosstanovitIgrovoiVvod();
        SostoyanieMenyu.GameMenuOpen = false;
    }

    void Start()
    {
        SozdatLuchiMenyu();
        SostoyanieMenyu.GameMenuOpen = false;
        if (panelMenu != null)
        {
            panelMenu.SetActive(false);
        }
        if (panelInformacii != null)
        {
            panelInformacii.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (kamera == null)
        {
            return;
        }

        // Меню всё время остаётся перед камерой, включая повороты головы.
        if (SostoyanieMenyu.GameMenuOpen && panelMenu != null && panelMenu.activeSelf)
        {
            RazmestitPanel(panelMenu.transform, rasstoyanieDoMenyu, Vector3.zero);
        }

        if (panelKratkihPodskazok != null && panelKratkihPodskazok.activeSelf)
        {
            Transform panel = panelKratkihPodskazok.transform;
            panel.position = kamera.position + kamera.forward * rasstoyanieDoPodskazok
                             + kamera.right * 0.42f - kamera.up * 0.34f;
            panel.rotation = Quaternion.LookRotation(kamera.forward, kamera.up);
        }
    }

    private void DobavitSlushateli()
    {
        if (knopkaProdolzhit != null) knopkaProdolzhit.onClick.AddListener(Zakryt);
        if (knopkaInformaciya != null) knopkaInformaciya.onClick.AddListener(PokazatInformaciyu);
        if (knopkaPodskazki != null) knopkaPodskazki.onClick.AddListener(PereklyuchitPodskazki);
        if (knopkaGlavnoeMenu != null) knopkaGlavnoeMenu.onClick.AddListener(VGlavnoeMenu);
        if (knopkaVyyti != null) knopkaVyyti.onClick.AddListener(VyytiIzIgry);
        if (knopkaNazadOtInformacii != null) knopkaNazadOtInformacii.onClick.AddListener(SkrytInformaciyu);
    }

    private void UdalitSlushateli()
    {
        if (knopkaProdolzhit != null) knopkaProdolzhit.onClick.RemoveListener(Zakryt);
        if (knopkaInformaciya != null) knopkaInformaciya.onClick.RemoveListener(PokazatInformaciyu);
        if (knopkaPodskazki != null) knopkaPodskazki.onClick.RemoveListener(PereklyuchitPodskazki);
        if (knopkaGlavnoeMenu != null) knopkaGlavnoeMenu.onClick.RemoveListener(VGlavnoeMenu);
        if (knopkaVyyti != null) knopkaVyyti.onClick.RemoveListener(VyytiIzIgry);
        if (knopkaNazadOtInformacii != null) knopkaNazadOtInformacii.onClick.RemoveListener(SkrytInformaciyu);
    }

    private void NazhatieMenu(InputAction.CallbackContext context)
    {
        if (panelMenu != null && panelMenu.activeSelf)
        {
            Zakryt();
        }
        else
        {
            Otkryt();
        }
    }

    public void Otkryt()
    {
        if (SostoyanieMenyu.GameMenuOpen)
        {
            return;
        }

        if (zakryvaemyePaneli != null)
        {
            for (int i = 0; i < zakryvaemyePaneli.Length; i++)
            {
                if (zakryvaemyePaneli[i] != null)
                {
                    zakryvaemyePaneli[i].Zakryt();
                }
            }
        }

        SostoyanieMenyu.GameMenuOpen = true;
        ZablokirovatIgrovoiVvod();
        VklyuchitLuchiMenyu();
        if (panelInformacii != null)
        {
            panelInformacii.SetActive(false);
        }
        if (panelMenu != null)
        {
            panelMenu.SetActive(true);
            RazmestitPanel(panelMenu.transform, rasstoyanieDoMenyu, Vector3.zero);
        }
    }

    public void Zakryt()
    {
        if (panelMenu != null)
        {
            panelMenu.SetActive(false);
        }
        if (panelInformacii != null)
        {
            panelInformacii.SetActive(false);
        }
        SostoyanieMenyu.GameMenuOpen = false;
        VyklyuchitLuchiMenyu();
        VosstanovitIgrovoiVvod();
    }

    // Отключаем игровой ввод, оставляя только действия, необходимые меню.
    private void ZablokirovatIgrovoiVvod()
    {
        zablokirovannyeDeistviya.Clear();
        if (naborDeistvii == null)
        {
            return;
        }

        foreach (InputActionMap karta in naborDeistvii.actionMaps)
        {
            bool etoLokomotsiya = karta.name == "XRI Left Locomotion" ||
                                  karta.name == "XRI Right Locomotion";
            bool etoZhesty = karta.name == "Touchscreen Gestures";
            bool etoVzaimodeistvie = karta.name == "XRI Left Interaction" ||
                                     karta.name == "XRI Right Interaction";

            foreach (InputAction deistvie in karta.actions)
            {
                bool blokirovat = etoLokomotsiya || etoZhesty ||
                                  (etoVzaimodeistvie && !EtoDeistvieMenyuIliUI(deistvie));
                if (blokirovat && deistvie.enabled)
                {
                    zablokirovannyeDeistviya.Add(deistvie);
                    deistvie.Disable();
                }
            }
        }
    }

    private static bool EtoDeistvieMenyuIliUI(InputAction deistvie)
    {
        switch (deistvie.name)
        {
            case "Select":
            case "Select Value":
            case "UI Press":
            case "UI Press Value":
            case "UI Scroll":
            case "OpenGameMenu":
                return true;
            default:
                return false;
        }
    }

    private void VosstanovitIgrovoiVvod()
    {
        for (int i = 0; i < zablokirovannyeDeistviya.Count; i++)
        {
            InputAction deistvie = zablokirovannyeDeistviya[i];
            if (deistvie != null && !deistvie.enabled)
            {
                deistvie.Enable();
            }
        }
        zablokirovannyeDeistviya.Clear();
    }

    private void SozdatLuchiMenyu()
    {
        string[] ruki = { "Left", "Right" };
        Transform[] vseObekty = FindObjectsOfType<Transform>(true);
        XRInteractionManager menedzher = FindObjectOfType<XRInteractionManager>();

        for (int i = 0; i < ruki.Length; i++)
        {
            string imyaKontrollera = ruki[i] + " Controller";
            Transform kontroller = null;
            for (int j = 0; j < vseObekty.Length; j++)
            {
                if (vseObekty[j].name == imyaKontrollera)
                {
                    kontroller = vseObekty[j];
                    break;
                }
            }
            if (kontroller == null)
            {
                Debug.LogError("IgrovoeMenu: не найден объект '" + imyaKontrollera + "'.", this);
                continue;
            }

            NearFarInteractor obychnyi = kontroller.GetComponentInChildren<NearFarInteractor>(true);
            GameObject obyektLucha = new GameObject("Menu Ray - " + imyaKontrollera);
            obyektLucha.SetActive(false);
            obyektLucha.transform.SetParent(kontroller, false);
            obyektLucha.transform.localPosition = Vector3.zero;
            obyektLucha.transform.localRotation = Quaternion.identity;
            obyektLucha.transform.localScale = Vector3.one;

            XRRayInteractor luch = obyektLucha.AddComponent<XRRayInteractor>();
            luch.interactionManager = menedzher;
            luch.rayOriginTransform = obyektLucha.transform;
            luch.lineType = XRRayInteractor.LineType.StraightLine;
            luch.maxRaycastDistance = 30f;
            luch.raycastMask = ~0;
            luch.interactionLayers = 0;
            luch.enableUIInteraction = true;
            luch.manipulateAttachTransform = false;

            InputActionMap karta = naborDeistvii.FindActionMap("XRI " + ruki[i] + " Interaction");
            luch.selectInput = new XRInputButtonReader
            {
                inputActionReferencePerformed = InputActionReference.Create(karta.FindAction("Select")),
                inputActionReferenceValue = InputActionReference.Create(karta.FindAction("Select Value"))
            };
            luch.uiPressInput = new XRInputButtonReader
            {
                inputActionReferencePerformed = InputActionReference.Create(karta.FindAction("UI Press")),
                inputActionReferenceValue = InputActionReference.Create(karta.FindAction("UI Press Value"))
            };
            luch.uiScrollInput = new XRInputValueReader<Vector2>
            {
                inputActionReference = InputActionReference.Create(karta.FindAction("UI Scroll"))
            };

            LineRenderer liniya = obyektLucha.AddComponent<LineRenderer>();
            liniya.useWorldSpace = true;
            liniya.startWidth = 0.008f;
            liniya.endWidth = 0.003f;
            liniya.numCapVertices = 4;
            liniya.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            liniya.receiveShadows = false;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                liniya.sharedMaterial = new Material(shader);
                liniya.sharedMaterial.color = new Color(0.35f, 0.9f, 1f, 1f);
            }

            XRInteractorLineVisual vizual = obyektLucha.AddComponent<XRInteractorLineVisual>();
            vizual.lineWidth = 0.008f;
            vizual.lineOriginTransform = obyektLucha.transform;
            Gradient cveta = new Gradient();
            cveta.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.35f, 0.9f, 1f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            vizual.validColorGradient = cveta;
            vizual.invalidColorGradient = cveta;

            luchiMenyu.Add(new LuchMenyu
            {
                obychnyiInteractor = obychnyi,
                interactorMenyu = luch,
                obychnyiBylVkljuchen = obychnyi != null && obychnyi.enabled
            });
        }
    }

    private void VklyuchitLuchiMenyu()
    {
        for (int i = 0; i < luchiMenyu.Count; i++)
        {
            LuchMenyu luch = luchiMenyu[i];
            if (luch.interactorMenyu == null) continue;
            if (luch.obychnyiInteractor != null)
            {
                luch.obychnyiBylVkljuchen = luch.obychnyiInteractor.enabled;
                luch.obychnyiInteractor.enabled = false;
            }
            luch.interactorMenyu.gameObject.SetActive(true);
        }
    }

    private void VyklyuchitLuchiMenyu()
    {
        for (int i = 0; i < luchiMenyu.Count; i++)
        {
            LuchMenyu luch = luchiMenyu[i];
            if (luch.interactorMenyu != null) luch.interactorMenyu.gameObject.SetActive(false);
            if (luch.obychnyiInteractor != null) luch.obychnyiInteractor.enabled = luch.obychnyiBylVkljuchen;
        }
    }

    public void PokazatInformaciyu()
    {
        if (panelInformacii != null)
        {
            panelInformacii.SetActive(true);
        }
    }

    public void SkrytInformaciyu()
    {
        if (panelInformacii != null)
        {
            panelInformacii.SetActive(false);
        }
    }

    public void PereklyuchitPodskazki()
    {
        if (panelKratkihPodskazok != null)
        {
            panelKratkihPodskazok.SetActive(!panelKratkihPodskazok.activeSelf);
        }
    }

    private void RazmestitPanel(Transform panel, float rasstoyanie, Vector3 smeshchenie)
    {
        if (panel == null || kamera == null)
        {
            return;
        }

        panel.position = kamera.position + kamera.forward * rasstoyanie + smeshchenie;
        panel.rotation = Quaternion.LookRotation(kamera.forward, kamera.up);
    }

    public void VGlavnoeMenu()
    {
        Zakryt();
        Time.timeScale = 1f;
        SceneManager.LoadScene(imyaScenyGlavnogoMenyu);
    }

    public void VyytiIzIgry()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
