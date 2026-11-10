using UnityEngine;
using UnityEngine.UI;

// Заглушка настроек инструмента "Газон": выбор между Кистью и Ластиком,
// и слайдер плотности, который виден только при выбранной Кисти.
// Сама покраска/стирание газона сюда пока не входит.
public class GazonInstrumenty : MonoBehaviour
{
    [Header("Кнопки выбора инструмента")]
    public Button knopkaKist;
    public Button knopkaLastik;

    [Header("Панель со слайдером плотности")]
    public GameObject panelPlotnosti;
    public Slider slaiderPlotnosti;

    void Start()
    {
        knopkaKist.onClick.AddListener(VybratKist);
        knopkaLastik.onClick.AddListener(VybratLastik);

        VybratKist();
    }

    private void VybratKist()
    {
        panelPlotnosti.SetActive(true);
    }

    private void VybratLastik()
    {
        panelPlotnosti.SetActive(false);
    }
}