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

    [Header("Сколько колонок, если элементы лежат сеткой (0 или 1 = обычный список столбиком)")]
    public int kolonokVSetke = 1;

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

    // Есть ли у этого меню сетка (больше одной колонки)
    public bool EToSetka()
    {
        return kolonokVSetke > 1;
    }

    // Шаг вниз. В обычном списке - следующий элемент, в сетке - вниз по колонке (по кругу)
    public void ShagVniz()
    {
        if (!EToSetka())
        {
            SleduyuschayaVkladka();
            return;
        }

        ShagPoKolonke(1);
    }

    // Шаг вверх. В обычном списке - предыдущий элемент, в сетке - вверх по колонке (по кругу)
    public void ShagVverkh()
    {
        if (!EToSetka())
        {
            PredydushayaVkladka();
            return;
        }

        ShagPoKolonke(-1);
    }

    // Шаг вправо по ряду. Возвращает true, если курсор реально сдвинулся
    public bool ShagVpravo()
    {
        int vsego = KolichestvoVkladok();
        if (!EToSetka() || vsego == 0)
        {
            return false;
        }

        int kolonka = tekushayaVkladka % kolonokVSetke;
        bool est_kuda_idti = (kolonka < kolonokVSetke - 1) && (tekushayaVkladka + 1 < vsego);
        if (est_kuda_idti)
        {
            OtkrytVkladku(tekushayaVkladka + 1);
            return true;
        }

        return false;
    }

    // Шаг влево по ряду. Возвращает true, если курсор реально сдвинулся
    // (false = стоим в левой колонке, сдвигаться некуда - значит, можно выходить из категории)
    public bool ShagVlevo()
    {
        if (!EToSetka() || KolichestvoVkladok() == 0)
        {
            return false;
        }

        int kolonka = tekushayaVkladka % kolonokVSetke;
        if (kolonka > 0)
        {
            OtkrytVkladku(tekushayaVkladka - 1);
            return true;
        }

        return false;
    }

    // Движение по колонке вверх/вниз по кругу (napravlenie = 1 вниз, -1 вверх)
    private void ShagPoKolonke(int napravlenie)
    {
        int vsego = KolichestvoVkladok();
        if (vsego == 0) return;

        int kolonka = tekushayaVkladka % kolonokVSetke;
        int stroka = tekushayaVkladka / kolonokVSetke;
        int strokVsego = (vsego + kolonokVSetke - 1) / kolonokVSetke;

        int noviyNomer = tekushayaVkladka;

        // Если в последней строке не хватает ячейки в нашей колонке - перескакиваем её
        for (int popytka = 0; popytka < strokVsego; popytka++)
        {
            stroka = (stroka + napravlenie + strokVsego) % strokVsego;
            int kandidat = stroka * kolonokVSetke + kolonka;

            if (kandidat < vsego)
            {
                noviyNomer = kandidat;
                break;
            }
        }

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