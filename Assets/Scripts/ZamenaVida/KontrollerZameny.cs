using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Склеивает панель замены (PanelZameny), панель действий (InventarDeystviy) и логику замены.
// Ни InventarDeystviy, ни DzhoystikNavigatsiyaInventarya менять не нужно:
//  - кнопка "Заменить вид" открывает панель через onClick, на который мы подписываемся здесь;
//  - пока панель замены открыта, навигация по инвентарю выключается, чтобы один и тот же
//    джойстик не управлял двумя списками сразу.
//
// ВАЖНО: вешать на объект, который никогда не выключается (например, InventoryManager),
// а не на саму панель PanelZameny.
public class KontrollerZameny : MonoBehaviour
{
    [Header("Панель действий над объектом (там кнопка 'Заменить вид' и текущий объект)")]
    public InventarDeystviy inventar;

    [Header("Панель выбора вида")]
    public PanelZameny panelZameny;

    [Header("Навигация по инвентарю (выключается, пока открыта панель замены). Можно оставить пустым")]
    public DzhoystikNavigatsiyaInventarya navigatsiyaInventarya;

    [Header("Закрывать панель сразу после замены (иначе остаётся открытой, галочка переезжает на новый вид)")]
    public bool zakryvatPosleZameny = false;

    [Header("Сколько секунд максимум ждать нейтрали стика после выхода из панели")]
    public float maksOzhidanieNeytrali = 1.5f;

    private Coroutine ozhidanieNeytrali;

    void OnEnable()
    {
        if (inventar != null && inventar.knopkaZamenitVid != null)
        {
            inventar.knopkaZamenitVid.onClick.AddListener(OtkrytPanel);
        }

        if (panelZameny != null)
        {
            panelZameny.VidVybran += ObrabotatVybor;
            panelZameny.Zakryta += ObrabotatZakrytie;
        }
    }

    void OnDisable()
    {
        if (inventar != null && inventar.knopkaZamenitVid != null)
        {
            inventar.knopkaZamenitVid.onClick.RemoveListener(OtkrytPanel);
        }

        if (panelZameny != null)
        {
            panelZameny.VidVybran -= ObrabotatVybor;
            panelZameny.Zakryta -= ObrabotatZakrytie;
        }
    }

    private void OtkrytPanel()
    {
        if (inventar == null || panelZameny == null || inventar.tekushiyObyekt == null)
        {
            return;
        }

        // Если ждали нейтрали после прошлого закрытия - отменяем, навигацию снова выключаем
        if (ozhidanieNeytrali != null)
        {
            StopCoroutine(ozhidanieNeytrali);
            ozhidanieNeytrali = null;
        }

        if (navigatsiyaInventarya != null)
        {
            navigatsiyaInventarya.enabled = false;
        }

        panelZameny.Otkryt(inventar.tekushiyObyekt);
    }

    private void ObrabotatVybor(VidZameny vid)
    {
        if (inventar == null || vid == null)
        {
            return;
        }

        Nasazhdenie staroe = inventar.tekushiyObyekt;
        if (staroe == null)
        {
            return;
        }

        // Выбран тот же вид, что уже стоит - ничего не делаем
        if (string.Equals(staroe.vidNasazhdeniya, vid.nazvanie, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Nasazhdenie novoe = ZamenaNasazhdeniya.Zamenit(staroe, vid.prefab, vid.nazvanie);
        if (novoe == null)
        {
            return;
        }

        // Инвентарь обновит название, фиксацию и т.д. уже для нового объекта
        inventar.UstanovitTekushiyObyekt(novoe);

        if (zakryvatPosleZameny)
        {
            panelZameny.Zakryt();
        }
        else
        {
            panelZameny.UstanovitObyekt(novoe);
        }
    }

    private void ObrabotatZakrytie()
    {
        if (!gameObject.activeInHierarchy)
        {
            VernutNavigatsiyu();
            return;
        }

        if (ozhidanieNeytrali != null)
        {
            StopCoroutine(ozhidanieNeytrali);
        }

        ozhidanieNeytrali = StartCoroutine(VernutNavigatsiyuPosleNeytrali());
    }

    // После выхода свайпом стик ещё отклонён. Если сразу включить навигацию инвентаря,
    // она примет этот же наклон за новый шаг. Поэтому ждём возврата стика в нейтраль.
    private IEnumerator VernutNavigatsiyuPosleNeytrali()
    {
        float konec = Time.unscaledTime + maksOzhidanieNeytrali;

        while (Time.unscaledTime < konec && panelZameny != null && !panelZameny.DzhoystikVNeytrali())
        {
            yield return null;
        }

        ozhidanieNeytrali = null;
        VernutNavigatsiyu();
    }

    private void VernutNavigatsiyu()
    {
        // Фокус возвращаем на кнопку "Заменить вид", откуда пришли
        if (inventar != null && inventar.knopkaZamenitVid != null
            && inventar.knopkaZamenitVid.gameObject.activeInHierarchy
            && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(inventar.knopkaZamenitVid.gameObject);
        }

        if (navigatsiyaInventarya != null)
        {
            navigatsiyaInventarya.enabled = true;
        }
    }
}
