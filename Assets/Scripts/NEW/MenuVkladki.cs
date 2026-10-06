using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Переключает текущий выбранный элемент в списке (вкладки категорий,
// или вперемешку кнопки и слайдер внутри категории - например, Кисть/Ластик/Плотность).
public class MenuVkladki : MonoBehaviour
{
    [Header("Элементы списка по порядку (кнопки и/или слайдеры)")]
    public Selectable[] elementyVkladok;

    [Header("Панели, которые показываются/прячутся по индексу (необязательно, может быть короче elementyVkladok или вообще пустым)")]
    public GameObject[] paneliVkladok;

    [Header("Номер элемента, который выбран при старте")]
    public int nachalnayaVkladka = 0;

    private int tekushayaVkladka;

    // Позволяет другим скриптам узнать, какой элемент сейчас выбран
    public int TekushayaVkladka
    {
        get { return tekushayaVkladka; }
    }

    void Start()
    {
        // Вешаем обработчик клика на каждую кнопку (у слайдеров onClick нет - пропускаем их)
        if (elementyVkladok != null)
        {
            for (int i = 0; i < elementyVkladok.Length; i++)
            {
                int nomerVkladki = i; // локальная копия, чтобы каждая кнопка запомнила свой номер
                Button knopka = elementyVkladok[i] as Button;
                if (knopka != null)
                {
                    knopka.onClick.AddListener(() => OtkrytVkladku(nomerVkladki));
                }
            }
        }

        OtkrytVkladku(nachalnayaVkladka);
    }

    // Сколько всего пунктов в списке
    private int KolichestvoVkladok()
    {
        if (elementyVkladok != null && elementyVkladok.Length > 0)
        {
            return elementyVkladok.Length;
        }

        return paneliVkladok != null ? paneliVkladok.Length : 0;
    }

    // Делает текущим элемент с указанным номером. Панели (если заданы) показываются/прячутся
    // по своему индексу, но их может быть МЕНЬШЕ, чем элементов (например, у Газона
    // элементов три - Кисть/Ластик/Плотность, а отдельной панели для переключения вообще нет)
    public void OtkrytVkladku(int nomerVkladki)
    {
        int vsego = KolichestvoVkladok();
        if (nomerVkladki < 0 || vsego == 0 || nomerVkladki >= vsego)
        {
            return;
        }

        if (paneliVkladok != null)
        {
            for (int i = 0; i < paneliVkladok.Length; i++)
            {
                bool eto_nuzhnaya_vkladka = (i == nomerVkladki);
                paneliVkladok[i].SetActive(eto_nuzhnaya_vkladka);
            }
        }

        tekushayaVkladka = nomerVkladki;

        // Подсвечиваем сам элемент стандартным Selected Color Unity
        if (elementyVkladok != null && nomerVkladki < elementyVkladok.Length && elementyVkladok[nomerVkladki] != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(elementyVkladok[nomerVkladki].gameObject);
        }
    }

    // Переключает на следующий элемент по кругу
    public void SleduyuschayaVkladka()
    {
        int vsego = KolichestvoVkladok();
        if (vsego == 0) return;

        int noviyNomer = (tekushayaVkladka + 1) % vsego;
        OtkrytVkladku(noviyNomer);
    }

    // Переключает на предыдущий элемент по кругу
    public void PredydushayaVkladka()
    {
        int vsego = KolichestvoVkladok();
        if (vsego == 0) return;

        int noviyNomer = (tekushayaVkladka - 1 + vsego) % vsego;
        OtkrytVkladku(noviyNomer);
    }

    // Подтверждает текущий выбор - если это кнопка, вызывает её клик.
    // Если это слайдер - ничего не делает (слайдер двигается наклоном, не кликом).
    public void PodtverditViybor()
    {
        if (elementyVkladok == null || tekushayaVkladka >= elementyVkladok.Length)
        {
            return;
        }

        Button knopka = elementyVkladok[tekushayaVkladka] as Button;
        if (knopka != null)
        {
            knopka.onClick.Invoke();
        }
    }

    // Проверяет, является ли сейчас выбранный элемент слайдером
    public bool TekushiyElementEstSlider()
    {
        if (elementyVkladok == null || tekushayaVkladka >= elementyVkladok.Length)
        {
            return false;
        }

        return elementyVkladok[tekushayaVkladka] is Slider;
    }

    // Если текущий элемент - слайдер, двигает его значение на величину izmenenie
    public void IzmenitSlaiderEsliVybran(float izmenenie)
    {
        if (elementyVkladok == null || tekushayaVkladka >= elementyVkladok.Length)
        {
            return;
        }

        Slider slaider = elementyVkladok[tekushayaVkladka] as Slider;
        if (slaider != null)
        {
            slaider.value += izmenenie;
        }
    }
}