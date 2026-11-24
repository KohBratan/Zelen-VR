using UnityEngine;
using UnityEngine.UI;

public class UpravlenieKistyu : MonoBehaviour
{
    [Header("Ссылка на скрипт посадки цветов")]
    public Cveti posadka;

    [Header("Слайдеры в VR-меню")]
    public Slider slaiderDiametra;
    public Slider slaiderSkorosti;

    void Start()
    {
        // Настройка слайдера диаметра
        if (slaiderDiametra != null)
        {
            slaiderDiametra.minValue = 0.2f;
            slaiderDiametra.maxValue = 3.0f;
            slaiderDiametra.value = 0.5f; // Стартовый размер
            slaiderDiametra.onValueChanged.AddListener(IzmenitDiametr);
            IzmenitDiametr(slaiderDiametra.value);
        }

        // Настройка слайдера скорости
        if (slaiderSkorosti != null)
        {
            slaiderSkorosti.minValue = 0f;
            slaiderSkorosti.maxValue = 1f;
            slaiderSkorosti.value = 0.5f; // Стартовая скорость (довольно быстро)
            slaiderSkorosti.onValueChanged.AddListener(IzmenitSkorost);
            IzmenitSkorost(slaiderSkorosti.value);
        }
    }

    private void IzmenitDiametr(float val)
    {
        if (posadka != null) posadka.diametrKisti = val;
    }

    private void IzmenitSkorost(float val)
    {
        if (posadka != null)
        {
            // Инвертируем значение: ползунок вправо (1) = маленькая задержка (0.01с), 
            // ползунок влево (0) = большая задержка (0.3с)
            posadka.skorostPosadki = Mathf.Lerp(0.3f, 0.01f, val);
        }
    }
}