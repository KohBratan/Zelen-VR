using UnityEngine;
using UnityEngine.UI;

// Скрипт вешается на объект Canvas (ToolsMenuUI).
// Отвечает только за переключение вкладок (Клумбы/Газон/Деревья/Кусты) внутри меню.
public class MenuVkladki : MonoBehaviour
{
    [Header("Кнопки вкладок (по порядку)")]
    public Button[] knopkiVkladok;

    [Header("Панели вкладок (тот же порядок, что и кнопки)")]
    public GameObject[] paneliVkladok;

    [Header("Номер вкладки, которая открыта при старте")]
    public int nachalnayaVkladka = 0;

    private int tekushayaVkladka;

    // Позволяет другим скриптам узнать, какая вкладка сейчас открыта
    public int TekushayaVkladka
    {
        get { return tekushayaVkladka; }
    }

    void Start()
    {
        // Вешаем обработчик клика на каждую кнопку
        for (int i = 0; i < knopkiVkladok.Length; i++)
        {
            int nomerVkladki = i; // локальная копия, чтобы каждая кнопка запомнила свой номер
            knopkiVkladok[i].onClick.AddListener(() => OtkrytVkladku(nomerVkladki));
        }

        OtkrytVkladku(nachalnayaVkladka);
    }

    // Открывает вкладку с указанным номером и прячет остальные
    public void OtkrytVkladku(int nomerVkladki)
    {
        if (nomerVkladki < 0 || nomerVkladki >= paneliVkladok.Length)
        {
            return;
        }

        for (int i = 0; i < paneliVkladok.Length; i++)
        {
            bool eto_nuzhnaya_vkladka = (i == nomerVkladki);
            paneliVkladok[i].SetActive(eto_nuzhnaya_vkladka);
        }

        tekushayaVkladka = nomerVkladki;
    }

    // Переключает на следующую вкладку по кругу
    public void SleduyuschayaVkladka()
    {
        int noviyNomer = (tekushayaVkladka + 1) % paneliVkladok.Length;
        OtkrytVkladku(noviyNomer);
    }

    // Переключает на предыдущую вкладку по кругу
    public void PredydushayaVkladka()
    {
        int noviyNomer = (tekushayaVkladka - 1 + paneliVkladok.Length) % paneliVkladok.Length;
        OtkrytVkladku(noviyNomer);
    }

    // Подтверждает текущий выбор — вызывает клик той кнопки, которая сейчас выбрана
    // (пока это просто заглушка: сам клик ничего не спавнит, только помечает выбор)
    public void PodtverditViybor()
    {
        if (knopkiVkladok != null && tekushayaVkladka < knopkiVkladok.Length && knopkiVkladok[tekushayaVkladka] != null)
        {
            knopkiVkladok[tekushayaVkladka].onClick.Invoke();
        }
    }
}