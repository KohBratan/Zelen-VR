using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Управляет стартовой VR-сценой. Игровые механики в ней отключены,
// но XR-камера и лучи контроллеров продолжают работать с UI.
public class GlavnoeMenu : MonoBehaviour
{
    [Header("Игровая сцена")]
    public string imyaIgrovoiSceny = "BrightDay";

    [Header("Кнопки")]
    public Button knopkaIgrat;
    public Button knopkaVyyti;

    [Header("Игровые компоненты и панели, которые не нужны в главном меню")]
    public Behaviour[] otklyuchaemyeKomponenty;
    public GameObject[] skryvaemyePaneli;

    [Header("Размещение меню перед VR-камерой")]
    public Transform kamera;
    public Transform panel;
    public float rasstoyanieDoPaneli = 2.4f;

    void Awake()
    {
        if (otklyuchaemyeKomponenty != null)
        {
            for (int i = 0; i < otklyuchaemyeKomponenty.Length; i++)
            {
                if (otklyuchaemyeKomponenty[i] != null)
                {
                    otklyuchaemyeKomponenty[i].enabled = false;
                }
            }
        }

        if (skryvaemyePaneli != null)
        {
            for (int i = 0; i < skryvaemyePaneli.Length; i++)
            {
                if (skryvaemyePaneli[i] != null)
                {
                    skryvaemyePaneli[i].SetActive(false);
                }
            }
        }
    }

    void OnEnable()
    {
        if (knopkaIgrat != null)
        {
            knopkaIgrat.onClick.AddListener(Igrat);
        }
        if (knopkaVyyti != null)
        {
            knopkaVyyti.onClick.AddListener(Vyyti);
        }
    }

    void OnDisable()
    {
        if (knopkaIgrat != null)
        {
            knopkaIgrat.onClick.RemoveListener(Igrat);
        }
        if (knopkaVyyti != null)
        {
            knopkaVyyti.onClick.RemoveListener(Vyyti);
        }
    }

    void Start()
    {
        RazmestitPanel();
    }

    public void RazmestitPanel()
    {
        if (kamera == null || panel == null)
        {
            return;
        }

        Vector3 vperyod = kamera.forward;
        vperyod.y = 0f;
        if (vperyod.sqrMagnitude < 0.001f)
        {
            vperyod = Vector3.forward;
        }
        vperyod.Normalize();

        panel.position = kamera.position + vperyod * rasstoyanieDoPaneli;
        panel.rotation = Quaternion.LookRotation(vperyod, Vector3.up);
    }

    public void Igrat()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(imyaIgrovoiSceny);
    }

    public void Vyyti()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
