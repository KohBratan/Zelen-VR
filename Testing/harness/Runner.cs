using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

class Result
{
    public string Id, Group, Name, Status, Detail;
}

class Program
{
    static readonly List<Result> Results = new List<Result>();
    static int _unitN, _scenarioN;

    static void Check(string group, string name, bool ok, string detail = "")
    {
        bool sc = group == "scenario";
        string id = (sc ? "S-" : "U-") + (sc ? ++_scenarioN : ++_unitN).ToString("00");
        Results.Add(new Result {
            Id = id, Group = group, Name = name, Status = ok ? "PASS" : "FAIL", Detail = detail ?? ""
        });
    }

    static BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    static FieldInfo Field(object o, string name)
    {
        Type t = o.GetType();
        while (t != null)
        {
            var f = t.GetField(name, Flags);
            if (f != null) return f;
            t = t.BaseType;
        }
        return null;
    }

    static void Set(object o, string name, object value)
    {
        var f = Field(o, name);
        if (f == null) throw new Exception("No field " + name + " on " + o.GetType().Name);
        f.SetValue(o, value);
    }

    static object Get(object o, string name)
    {
        var f = Field(o, name);
        if (f == null) throw new Exception("No field " + name + " on " + o.GetType().Name);
        return f.GetValue(o);
    }

    static object Call(object o, string name, params object[] args)
    {
        Type t = o.GetType();
        while (t != null)
        {
            var m = t.GetMethod(name, Flags);
            if (m != null) return m.Invoke(o, args);
            t = t.BaseType;
        }
        throw new Exception("No method " + name + " on " + o.GetType().Name);
    }

    static GameObject Go(string name, params Type[] comps)
    {
        var go = new GameObject(name);
        foreach (var c in comps) go.AddComponent(c);
        return go;
    }

    static Nasazhdenie Plant(string vid, string cat, bool fix, Vector3 pos, Vector3 scale, GameObject prefab = null)
    {
        var go = Go(vid, typeof(Nasazhdenie));
        go.transform.position = pos;
        go.transform.localScale = scale;
        var n = go.GetComponent<Nasazhdenie>();
        n.vidNasazhdeniya = vid;
        n.kategoriya = cat;
        n.zafiksirovano = fix;
        n.sourcePrefab = prefab;
        n.ishodnyiRazmer = scale;
        return n;
    }

    static VidZameny Vid(string name, GameObject prefab)
    {
        return new VidZameny { nazvanie = name, prefab = prefab };
    }

    static YacheykaVida Cell(string name)
    {
        var btnGo = Go(name + "_btn", typeof(Button));
        var checkGo = Go(name + "_check");
        return new YacheykaVida {
            knopka = btnGo.GetComponent<Button>(),
            tekst = btnGo.AddComponent<Text>(),
            galochka = checkGo
        };
    }

    static EventSystem FreshEventSystem()
    {
        var es = Go("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        EventSystem.current = es;
        return es;
    }

    static bool Near(float a, float b, float eps = 0.0001f) { return Math.Abs(a - b) < eps; }

    static void Reset()
    {
        Debug.Clear();
        Physics.Handler = null;
        Physics.Calls = 0;
        Time.frameCount = 100;
        Time.unscaledTime = 0;
        VyborInstrumenta.TekushiyPrefab = null;
        SostoyanieMenyu.InventoryOpen = false;
        SostoyanieMenyu.ToolsMenuOpen = false;
        GameObject.All.Clear();
        FreshEventSystem();
    }

    static void Main()
    {
        UnitReplacement();
        UnitPanelZameny();
        UnitInventoryNavigation();
        UnitMenuTabs();
        UnitInventoryActions();
        UnitPlacementAndSelection();
        UnitRotationAndMisc();
        Scenarios();

        Console.WriteLine("=== RESULTS ===");
        foreach (var r in Results)
            Console.WriteLine(r.Id + "|" + r.Group + "|" + r.Name + "|" + r.Status + "|" + r.Detail);

        int passed = 0, failed = 0;
        foreach (var r in Results) { if (r.Status == "PASS") passed++; else failed++; }
        Console.WriteLine("=== SUMMARY total=" + Results.Count + " pass=" + passed + " fail=" + failed + " ===");

        string json = "[\n";
        for (int i = 0; i < Results.Count; i++)
        {
            var r = Results[i];
            json += "  {\"id\":\"" + r.Id + "\",\"group\":\"" + r.Group + "\",\"name\":\"" + r.Name.Replace("\"", "'") +
                    "\",\"status\":\"" + r.Status + "\",\"detail\":\"" + r.Detail.Replace("\"", "'") + "\"}" +
                    (i + 1 < Results.Count ? "," : "") + "\n";
        }
        json += "]";
        System.IO.File.WriteAllText("results.json", json, System.Text.Encoding.UTF8);
    }

    static void UnitReplacement()
    {
        Reset();
        string g = "unit-replacement";

        var old = Plant("Дуб", "Деревья", true, new Vector3(1, 2, 3), new Vector3(2, 2, 2));
        var res = ZamenaNasazhdeniya.Zamenit(null, Go("prefab"), "Липа");
        Check(g, "Замена с пустым старым объектом возвращает null", res == null, "результат=" + (res == null ? "null" : "объект"));

        res = ZamenaNasazhdeniya.Zamenit(old, null, "Липа");
        Check(g, "Замена без префаба возвращает null", res == null, "результат=" + (res == null ? "null" : "объект"));
        Check(g, "Предупреждение в лог при отсутствии префаба", Debug.Messages.Exists(x => x.StartsWith("WARN: ZamenaNasazhdeniya")), string.Join("; ", Debug.Messages));

        var prefab = Go("Липа_prefab", typeof(Nasazhdenie));
        prefab.GetComponent<Nasazhdenie>().vidNasazhdeniya = "Липа-из-префаба";
        var parent = new GameObject("container").transform;
        old.transform.parent = parent;
        res = ZamenaNasazhdeniya.Zamenit(old, prefab, "Липа");
        Check(g, "Успешная замена возвращает паспорт", res != null, "результат=" + (res == null ? "null" : res.vidNasazhdeniya));
        if (res != null)
        {
            Check(g, "Новый объект создан на позиции старого", res.transform.position == new Vector3(1,2,3), "pos=" + res.transform.position);
            Check(g, "Сохранён поворот", res.transform.rotation.x == old.transform.rotation.x && res.transform.rotation.w == old.transform.rotation.w, "rot ok");
            Check(g, "Сохранён родитель", res.transform.parent == parent, "parent=" + (res.transform.parent == null ? "null" : res.transform.parent.name));
            Check(g, "Сохранён масштаб (не из префаба)", res.transform.localScale == new Vector3(2,2,2), "scale=" + res.transform.localScale);
            Check(g, "Название вида обновлено", res.vidNasazhdeniya == "Липа", "vid=" + res.vidNasazhdeniya);
            Check(g, "Категория скопирована", res.kategoriya == "Деревья", "cat=" + res.kategoriya);
            Check(g, "Фиксация скопирована", res.zafiksirovano, "fixed=" + res.zafiksirovano);
            Check(g, "Старый объект скрыт до удаления", !old.gameObject.activeSelf, "active=" + old.gameObject.activeSelf);
            Check(g, "Старый объект помечен на удаление", old.gameObject.Destroyed, "destroyed=" + old.gameObject.Destroyed);
        }

        var plainPrefab = Go("Простой_prefab");
        var plain = Plant("Сосна", "Деревья", false, Vector3.zero, Vector3.one);
        var res2 = ZamenaNasazhdeniya.Zamenit(plain, plainPrefab, "Ель");
        Check(g, "Паспорт добавляется, если в префабе его нет", res2 != null && res2.GetType() == typeof(Nasazhdenie), "pasport=" + (res2 == null ? "null" : res2.GetType().Name));
    }

    static void UnitPanelZameny()
    {
        Reset();
        string g = "unit-replace-panel";

        var p = Go("PanelZameny", typeof(PanelZameny)).GetComponent<PanelZameny>();
        var cells = new[] { Cell("A"), Cell("B"), Cell("C"), Cell("D") };
        Set(p, "yacheyki", cells);
        p.kolonki = 2;
        p.krayaVyhoda = KrayVyhoda.Vse;
        var dubPref = Go("DubPref");
        var lipaPref = Go("LipaPref");
        p.katalog = new[] {
            new KategoriyaZameny { nazvanie = "Деревья", vidy = new[] { Vid("Дуб", dubPref), Vid("Липа", lipaPref) } }
        };
        Set(p, "pokazannyeVidy", p.katalog[0].vidy);
        var obj = Plant("ДУБ", "Деревья", false, Vector3.zero, Vector3.one);
        Set(p, "tekushiyObyekt", obj);

        var findCat = typeof(PanelZameny).GetMethod("NaytiKategoriyu", Flags);
        var cat = (KategoriyaZameny)findCat.Invoke(p, new object[] { "деревья" });
        Check(g, "Поиск категории без учёта регистра", cat != null && cat.nazvanie == "Деревья", "cat=" + (cat == null ? "null" : cat.nazvanie));
        cat = (KategoriyaZameny)findCat.Invoke(p, new object[] { "Газон" });
        Check(g, "Отсутствующая категория не находится", cat == null, "cat=" + (cat == null ? "null" : cat.nazvanie));

        var etoVid = typeof(PanelZameny).GetMethod("EtoTekushiyVid", Flags);
        Check(g, "Текущий вид сравнивается без регистра", (bool)etoVid.Invoke(p, new object[] { Vid("дуб", dubPref) }), "сравнение 'ДУБ' vs 'дуб'");

        var dostupna = typeof(PanelZameny).GetMethod("YacheykaDostupna", Flags);
        Check(g, "Недоступная ячейка за пределами каталога", !(bool)dostupna.Invoke(p, new object[] { 3 }), "индекс 3 при 2 видах");
        Check(g, "Отрицательный индекс ячейки недоступен", !(bool)dostupna.Invoke(p, new object[] { -1 }), "индекс -1");

        int fired = 0;
        VidZameny picked = null;
        p.VidVybran += v => { fired++; picked = v; };
        Call(p, "NazhatieYacheyki", 3);
        Check(g, "Клик по недоступной ячейке игнорируется", fired == 0, "событий=" + fired);
        Call(p, "NazhatieYacheyki", 1);
        Check(g, "Клик по доступной ячейке выдаёт событие", fired == 1 && picked != null && picked.nazvanie == "Липа", "событий=" + fired + ", вид=" + (picked == null ? "null" : picked.nazvanie));

        Set(p, "tekushayaYacheyka", 0);
        Call(p, "ObrabotatNaklon", new Vector2(1, 0));
        Check(g, "Движение вправо по сетке", (int)Get(p, "tekushayaYacheyka") == 1, "ячейка=" + Get(p, "tekushayaYacheyka"));
        Call(p, "ObrabotatNaklon", new Vector2(-1, 0));
        Check(g, "Движение влево по сетке", (int)Get(p, "tekushayaYacheyka") == 0, "ячейка=" + Get(p, "tekushayaYacheyka"));
        Call(p, "ObrabotatNaklon", new Vector2(-1, 0));
        Check(g, "Свайп за левый край закрывает панель", !p.gameObject.activeSelf, "active=" + p.gameObject.activeSelf);

        p.gameObject.SetActive(true);
        p.krayaVyhoda = KrayVyhoda.Nichego;
        Call(p, "ObrabotatNaklon", new Vector2(-1, 0));
        Check(g, "При запрещённом выходе панель не закрывается", p.gameObject.activeSelf, "active=" + p.gameObject.activeSelf);

        p.porogVozvrata = 0.3f;
        p.porogNaklona = 0.5f;
        Set(p, "zhdyomNeytral", true);
        Call(p, "SobytieDzhoystika", new InputAction.CallbackContext { value = new Vector2(0, 0.9f) });
        Check(g, "Повторный наклон до нейтрали игнорируется", (int)Get(p, "tekushayaYacheyka") == 0, "ячейка=" + Get(p, "tekushayaYacheyka"));
        Call(p, "SobytieDzhoystika", new InputAction.CallbackContext { value = Vector2.zero });
        Check(g, "Возврат в нейтраль сбрасывает блокировку", !(bool)Get(p, "zhdyomNeytral"), "zhdyom=" + Get(p, "zhdyomNeytral"));

        Time.frameCount = 500;
        Set(p, "kadrOtkrytiya", 500);
        fired = 0;
        Call(p, "NazhatieKlika", new InputAction.CallbackContext { value = Vector2.zero });
        Check(g, "Клик в кадре открытия панели игнорируется", fired == 0, "событий=" + fired);
        Time.frameCount = 501;
        Call(p, "NazhatieKlika", new InputAction.CallbackContext { value = Vector2.zero });
        Check(g, "Клик в следующем кадре подтверждает выбор", fired == 1, "событий=" + fired);
    }

    static void UnitInventoryNavigation()
    {
        Reset();
        string g = "unit-inventory-nav";

        SostoyanieMenyu.InventoryOpen = false;
        SostoyanieMenyu.ToolsMenuOpen = false;
        Check(g, "AnyMenuOpen=false когда все меню закрыты", !SostoyanieMenyu.AnyMenuOpen, "inventory=" + SostoyanieMenyu.InventoryOpen + ", tools=" + SostoyanieMenyu.ToolsMenuOpen);
        SostoyanieMenyu.ToolsMenuOpen = true;
        Check(g, "AnyMenuOpen=true при открытом меню инструментов", SostoyanieMenyu.AnyMenuOpen, "tools=true");
        SostoyanieMenyu.InventoryOpen = true;
        Check(g, "AnyMenuOpen=true при двух открытых меню", SostoyanieMenyu.AnyMenuOpen, "both=true");

        var nav = Go("nav", typeof(DzhoystikNavigatsiyaInventarya)).GetComponent<DzhoystikNavigatsiyaInventarya>();
        var e1 = Go("btn1", typeof(Button)).GetComponent<Selectable>();
        var e2 = Go("btn2", typeof(Button)).GetComponent<Selectable>();
        var e3 = Go("sld3", typeof(Slider)).GetComponent<Selectable>();
        e2.gameObject.SetActive(false);
        Set(nav, "elementyMenu", new[] { e1, e2, e3 });
        nav.shagSlaidera = 0.1f;
        nav.porogNaklona = 0.5f;
        var panel = Go("inventoryPanel");
        nav.panelMenu = panel;
        panel.SetActive(true);

        Call(nav, "PereklyuchitElement", 1);
        Check(g, "Переключение пропускает неактивный элемент", (int)Get(nav, "tekushiyIndeks") == 2, "индекс=" + Get(nav, "tekushiyIndeks"));
        Call(nav, "PereklyuchitElement", 1);
        Check(g, "Переключение по кругу возвращает на первый элемент", (int)Get(nav, "tekushiyIndeks") == 0, "индекс=" + Get(nav, "tekushiyIndeks"));
        Call(nav, "PereklyuchitElement", -1);
        Check(g, "Переключение назад с первого уходит на последний доступный", (int)Get(nav, "tekushiyIndeks") == 2, "индекс=" + Get(nav, "tekushiyIndeks"));

        var slider = (Slider)e3;
        slider.SetValueWithoutNotify(0.5f);
        Call(nav, "IzmenitSlaiderEsliVybran", 0.1f);
        Check(g, "Слайдер меняется на заданный шаг", Near(slider.value, 0.6f), "value=" + slider.value);
        Set(nav, "tekushiyIndeks", 0);
        float oldValue = slider.value;
        Call(nav, "IzmenitSlaiderEsliVybran", 0.1f);
        Check(g, "Нажатие на кнопку не меняет слайдер", Near(slider.value, oldValue), "value=" + slider.value);

        var nav2 = Go("nav2", typeof(DzhoystikNavigatsiyaInventarya)).GetComponent<DzhoystikNavigatsiyaInventarya>();
        nav2.porogNaklona = 0.5f;
        nav2.panelMenu = panel;
        Call(nav2, "SobytieDzhoystika", new InputAction.CallbackContext { value = new Vector2(0, 0.7f) });
        Check(g, "Наклон выше порога вызывает обработку", (bool)Get(nav2, "zhdyomNeytral"), "zhdyom=" + Get(nav2, "zhdyomNeytral"));
        Call(nav2, "SobytieDzhoystika", new InputAction.CallbackContext { value = new Vector2(0, 0.7f) });
        Check(g, "Второй наклон без нейтрали не обрабатывается", (int)Get(nav2, "tekushiyIndeks") == 0, "индекс=" + Get(nav2, "tekushiyIndeks"));

        var treePanel = Go("treePanel");
        var bushPanel = Go("bushPanel");
        nav.knopkiZamenyDerevya = new[] {
            MakeReplaceButton("Дуб", Go("dubPref")), MakeReplaceButton("Липа", Go("lipaPref"))
        };
        nav.knopkiZamenyKusty = new[] { MakeReplaceButton("Бук", Go("bukPref")) };
        nav.panelZamenyDerevya = treePanel;
        nav.panelZamenyKusty = bushPanel;
        nav.kolonokZamenyDerevya = 2;
        nav.kolonokZamenyKusty = 1;

        var prefDub = Go("dubPref2");
        var obj = Plant("Дуб", "Деревья", false, Vector3.zero, Vector3.one, prefDub);
        nav.OtkrytPanelZameny(obj);
        Check(g, "Режим замены включён для категории Деревья", (bool)Get(nav, "rezhimZameny") && treePanel.activeSelf, "panel=" + treePanel.activeSelf);
        Check(g, "Выбрана кнопка текущего источника", (int)Get(nav, "tekushiyIndeksZameny") == 0, "индекс=" + Get(nav, "tekushiyIndeksZameny"));
        Check(g, "Галочка стоит на текущем виде", nav.knopkiZamenyDerevya[0].metkaVybrano.activeSelf && !nav.knopkiZamenyDerevya[1].metkaVybrano.activeSelf, "метки=" + nav.knopkiZamenyDerevya[0].metkaVybrano.activeSelf + "," + nav.knopkiZamenyDerevya[1].metkaVybrano.activeSelf);

        var objBush = Plant("Бук", "Кусты", false, Vector3.zero, Vector3.one);
        nav.OtkrytPanelZameny(objBush);
        Check(g, "Для кустов открыта своя панель", bushPanel.activeSelf && !treePanel.activeSelf, "bush=" + bushPanel.activeSelf + ", tree=" + treePanel.activeSelf);

        var objBad = Plant("Газон", "Газон", false, Vector3.zero, Vector3.one);
        Debug.Clear();
        nav.OtkrytPanelZameny(objBad);
        Check(g, "Неподдерживаемая категория не открывает панель", !(bool)Get(nav, "rezhimZameny"), "rezhim=" + Get(nav, "rezhimZameny"));
        Debug.Clear();
        nav.OtkrytPanelZameny(null);
        Check(g, "Пустой объект не открывает панель замены", !(bool)Get(nav, "rezhimZameny"), "rezhim=" + Get(nav, "rezhimZameny"));

        nav.OtkrytPanelZameny(obj);
        Set(nav, "tekushiyIndeksZameny", 1);
        bool moved = (bool)Call(nav, "ShagPoRyaduZameny", 1);
        Check(g, "Сдвиг за правый край сетки запрещён", !moved, "moved=" + moved);
        moved = (bool)Call(nav, "ShagPoRyaduZameny", -1);
        Check(g, "Сдвиг влево внутри сетки разрешён", moved && (int)Get(nav, "tekushiyIndeksZameny") == 0, "index=" + Get(nav, "tekushiyIndeksZameny"));

        nav.ZakrytPanelZameny();
        Check(g, "Закрытие панели выключает режим замены", !(bool)Get(nav, "rezhimZameny") && !treePanel.activeSelf && !bushPanel.activeSelf, "rezhim=" + Get(nav, "rezhimZameny"));
    }

    static KnopkaZamenyVida MakeReplaceButton(string name, GameObject prefab)
    {
        var go = Go(name, typeof(KnopkaZamenyVida));
        var k = go.GetComponent<KnopkaZamenyVida>();
        k.prefabNasazhdeniya = prefab;
        k.metkaVybrano = Go(name + "_mark");
        return k;
    }

    static void UnitMenuTabs()
    {
        Reset();
        string g = "unit-menu-tabs";
        var m = Go("tabs", typeof(MenuVkladki)).GetComponent<MenuVkladki>();
        var b1 = Go("b1", typeof(Button)).GetComponent<Button>();
        var b2 = Go("b2", typeof(Button));
        var s3 = Go("s3", typeof(Slider));
        m.elementyVkladok = new Selectable[] { b1.GetComponent<Button>(), b2.GetComponent<Button>(), s3.GetComponent<Slider>() };
        m.paneliVkladok = new[] { Go("p1"), Go("p2"), Go("p3") };
        m.kolonokVSetke = 1;
        m.nachalnayaVkladka = 0;

        m.OtkrytVkladku(1);
        Check(g, "Выбор вкладки показывает только нужную панель", m.paneliVkladok[1].activeSelf && !m.paneliVkladok[0].activeSelf, "p1=" + m.paneliVkladok[0].activeSelf + ", p2=" + m.paneliVkladok[1].activeSelf);
        Check(g, "Текущая вкладка запомнена", m.TekushayaVkladka == 1, "tab=" + m.TekushayaVkladka);
        m.OtkrytVkladku(99);
        Check(g, "Неверный индекс вкладки игнорируется", m.TekushayaVkladka == 1, "tab=" + m.TekushayaVkladka);
        m.SleduyuschayaVkladka();
        Check(g, "Следующая вкладка", m.TekushayaVkladka == 2, "tab=" + m.TekushayaVkladka);
        m.SleduyuschayaVkladka();
        Check(g, "Переход по кругу на первую вкладку", m.TekushayaVkladka == 0, "tab=" + m.TekushayaVkladka);
        m.PredydushayaVkladka();
        Check(g, "Предыдущая вкладка по кругу", m.TekushayaVkladka == 2, "tab=" + m.TekushayaVkladka);

        Check(g, "Слайдер определяется как текущий элемент", m.TekushiyElementEstSlider(), "tab=2");
        var sl = (Slider)m.elementyVkladok[2];
        sl.SetValueWithoutNotify(0.4f);
        m.IzmenitSlaiderEsliVybran(0.2f);
        Check(g, "Слайдер вкладки меняется на шаг", Near(sl.value, 0.6f), "value=" + sl.value);
        int clicks = 0;
        ((Button)m.elementyVkladok[0]).onClick.AddListener(() => clicks++);
        m.OtkrytVkladku(0);
        m.PodtverditViybor();
        Check(g, "Подтверждение на кнопке вызывает клик", clicks == 1, "clicks=" + clicks);
        m.OtkrytVkladku(2);
        m.PodtverditViybor();
        Check(g, "Подтверждение на слайдере ничего не делает", clicks == 1, "clicks=" + clicks);

        m.kolonokVSetke = 2;
        m.elementyVkladok = new Selectable[] {
            Go("g1", typeof(Button)).GetComponent<Button>(), Go("g2", typeof(Button)).GetComponent<Button>(),
            Go("g3", typeof(Button)).GetComponent<Button>(), Go("g4", typeof(Button)).GetComponent<Button>()
        };
        m.OtkrytVkladku(0);
        Check(g, "Шаг вправо в сетке", m.ShagVpravo() && m.TekushayaVkladka == 1, "tab=" + m.TekushayaVkladka);
        Check(g, "Шаг вправо на правом краю возвращает false", !m.ShagVpravo(), "tab=" + TekushayaVкладкаGuard(m));
        Check(g, "Шаг влево в сетке", m.ShagVlevo() && TekushayaVкладкаGuard(m) == 0, "tab=" + TekushayaVкладкаGuard(m));
        Check(g, "Шаг влево на левом краю возвращает false", !m.ShagVlevo(), "tab=" + TekushayaVкладкаGuard(m));
        m.ShagVniz();
        Check(g, "Шаг вниз по колонке", m.TekushayaVkladka == 2, "tab=" + m.TekushayaVkladka);
        m.ShagVverkh();
        Check(g, "Шаг вверх по колонке", m.TekushayaVkladka == 0, "tab=" + m.TekushayaVkladka);

        m.kolonokVSetke = 1;
        Check(g, "Обычный список не считается сеткой", !m.EToSetka(), "kolonok=1");
    }

    static int TekushayaVкладкаGuard(MenuVkladki m) { return m.TekushayaVkladka; }

    static InventarDeystviy MakeInventory(out GameObject panelGo)
    {
        var go = Go("inventar", typeof(InventarDeystviy));
        var inv = go.GetComponent<InventarDeystviy>();
        Set(inv, "knopkaPovernut", Go("bRot", typeof(Button)).GetComponent<Button>());
        Set(inv, "knopkaRastyanut", Go("bStretch", typeof(Button)).GetComponent<Button>());
        Set(inv, "knopkaZafiksirovat", Go("bFix", typeof(Button)).GetComponent<Button>());
        Set(inv, "knopkaUdalit", Go("bDel", typeof(Button)).GetComponent<Button>());
        Set(inv, "knopkaZamenitVid", Go("bReplace", typeof(Button)).GetComponent<Button>());
        Set(inv, "tekstNazvaniyaObyekta", Go("tName", typeof(Text)).GetComponent<Text>());
        Set(inv, "tekstKnopkiZafiksirovat", Go("tFix", typeof(Text)).GetComponent<Text>());
        Set(inv, "tekstKnopkiPovernut", Go("tRot", typeof(Text)).GetComponent<Text>());
        Set(inv, "panelRazmer", Go("sizePanel"));
        Set(inv, "slaiderRazmer", Go("sizeSlider", typeof(Slider)).GetComponent<Slider>());
        panelGo = Go("inventoryPanel");
        return inv;
    }

    static void UnitInventoryActions()
    {
        Reset();
        string g = "unit-inventory-actions";
        GameObject pg;
        var inv = MakeInventory(out pg);
        inv.slaiderRazmer = (Slider)Get(inv, "slaiderRazmer");
        var textName = (Text)Get(inv, "tekstNazvaniyaObyekta");
        var textFix = (Text)Get(inv, "tekstKnopkiZafiksirovat");
        var bStretch = (Button)Get(inv, "knopkaRastyanut");
        var bRot = (Button)Get(inv, "knopkaPovernut");

        inv.UstanovitTekushiyObyekt(null);
        Check(g, "Пустой выбор отключает кнопки", !bRot.interactable && !((Button)Get(inv,"knopkaUdalit")).interactable, "rot=" + bRot.interactable);
        Check(g, "Пустой выбор показывает заглушку", textName.text == "Ничего не выбрано", "text=" + textName.text);
        Check(g, "Пустой выбор скрывает панель размера", !((GameObject)Get(inv,"panelRazmer")).activeSelf, "panel=" + ((GameObject)Get(inv,"panelRazmer")).activeSelf);

        var tree = Plant("Дуб", "Деревья", false, new Vector3(5,0,0), new Vector3(1,1,1));
        inv.UstanovitTekushiyObyekt(tree);
        Check(g, "Имя объекта показано в инвентаре", textName.text == "Дуб", "text=" + textName.text);
        Check(g, "Кнопка растягивания скрыта для дерева", !bStretch.gameObject.activeSelf, "active=" + bStretch.gameObject.activeSelf);
        Check(g, "Текст фиксации для незафиксированного объекта", textFix.text == "Зафиксировать", "text=" + textFix.text);

        var lawn = Plant("Газон", "Газон", true, Vector3.zero, new Vector3(1,1,1));
        inv.UstanovitTekushiyObyekt(lawn);
        Check(g, "Кнопка растягирования показана для газона", bStretch.gameObject.activeSelf, "active=" + bStretch.gameObject.activeSelf);
        Check(g, "Текст фиксации для зафиксированного объекта", textFix.text == "Открепить", "text=" + textFix.text);

        float z0 = lawn.transform.localScale.z;
        inv.shagRastyagivaniya = 0.2f;
        Call(inv, "Rastyanut");
        Check(g, "Растягивание увеличивает длину на шаг", Near(lawn.transform.localScale.z, z0 + 0.2f), "z=" + lawn.transform.localScale.z);

        inv.slaiderRazmer.SetValueWithoutNotify(0.5f);
        Call(inv, "IzmenitRazmer", 0.5f);
        Check(g, "Середина слайдера = исходный размер", Near(lawn.transform.localScale.x, 1f), "scale=" + lawn.transform.localScale);
        Call(inv, "IzmenitRazmer", 0f);
        Check(g, "Минимальное значение слайдера уменьшает объект", lawn.transform.localScale.x < 1f, "scale=" + lawn.transform.localScale.x);
        Call(inv, "IzmenitRazmer", 1f);
        Check(g, "Максимальное значение слайдера увеличивает объект", lawn.transform.localScale.x > 1f, "scale=" + lawn.transform.localScale.x);

        inv.UstanovitTekushiyObyekt(tree);
        float rot0 = tree.transform.rotation.y;
        inv.Vrashat(45f);
        Check(g, "Вращение без режима не применяется", Near(tree.transform.rotation.y, rot0), "rot=" + tree.transform.rotation.y);
        Call(inv, "Povernut");
        Check(g, "Режим вращения включается", inv.RezhimVrascheniya, "mode=" + inv.RezhimVrascheniya);
        inv.Vrashat(45f);
        Check(g, "Вращение применяется в режиме", tree.transform.rotation.y > rot0, "rot=" + tree.transform.rotation.y);
        inv.ZakonchitVrashenie();
        Check(g, "Режим вращения выключается", !inv.RezhimVrascheniya, "mode=" + inv.RezhimVrascheniya);

        var closer = Go("closer", typeof(PanelPereklyuchatel)).GetComponent<PanelPereklyuchatel>();
        closer.panel = pg;
        closer.Otkryt();
        Set(inv, "pereklyuchatelInventarya", closer);
        Call(inv, "PereklyuchitFiksatsiyu");
        Check(g, "Фиксация закрывает инвентарь", !pg.activeSelf, "panel=" + pg.activeSelf);

        inv.UstanovitTekushiyObyekt(tree);
        Call(inv, "Udalit");
        Check(g, "Удаление снимает выбор", inv.tekushiyObyekt == null, "object=" + (inv.tekushiyObyekt == null ? "null" : "есть"));
        Check(g, "Удалённый объект помечен на уничтожение", tree.gameObject.Destroyed, "destroyed=" + tree.gameObject.Destroyed);
    }

    static void UnitPlacementAndSelection()
    {
        Reset();
        string g = "unit-placement";

        var go = Go("posadka", typeof(PosadkaDereva));
        var p = go.GetComponent<PosadkaDereva>();
        var lineStub = Go("line", typeof(LineRenderer)).GetComponent<LineRenderer>();
        Set(p, "luch", lineStub);
        Set(p, "materialLuchaKopiya", lineStub.material);
        Set(p, "tochkaStarta", null);
        p.dlinaLucha = 30f;
        p.tsvetPopal = Color.green;
        p.tsvetNePopal = Color.red;
        p.materialPrizraka = new Material("ghost");

        go.transform.position = new Vector3(0, 1, 0);
        go.transform.forward = new Vector3(0, 0, 1);
        var prefab = Go("Дуб", typeof(Nasazhdenie));
        VyborInstrumenta.TekushiyPrefab = prefab;
        Physics.Handler = null;

        Call(p, "LateUpdate");
        var line = (LineRenderer)Get(p, "luch");
        Check(g, "Без попадания луч красный", line.material.GetColor("_BaseColor") == Color.red, "color=" + line.material.GetColor("_BaseColor"));
        Check(g, "Без попадания конец луча на максимальной длине", Near(line.positions[1].z, 30f), "end=" + line.positions[1]);
        Check(g, "Без попадания призрак не создан", Get(p, "prizrak") == null, "ghost=" + Get(p, "prizrak"));

        Debug.Clear();
        Call(p, "NazhatieTriggera", new InputAction.CallbackContext());
        Check(g, "Триггер без попадания не сажает дерево", !Debug.Messages.Exists(x => x.StartsWith("LOG: Посажено")), string.Join(";", Debug.Messages));

        var hitGo = Go("terrainHit", typeof(Collider));
        Physics.Handler = (o, d, max, mask, qti) => new RaycastHit {
            point = new Vector3(0, 0, 10), collider = hitGo.GetComponent<Collider>(), distance = 10f
        };
        Call(p, "LateUpdate");
        Check(g, "Попадание по террейну создаёт призрак", Get(p, "prizrak") != null && ((GameObject)Get(p, "prizrak")).activeSelf, "ghost=" + Get(p, "prizrak"));
        Check(g, "Призрак стоит в точке попадания", ((GameObject)Get(p, "prizrak")).transform.position == new Vector3(0,0,10), "pos=" + ((GameObject)Get(p, "prizrak")).transform.position);
        Check(g, "Конец луча в точке попадания", Near(line.positions[1].z, 10f), "end=" + line.positions[1]);
        Check(g, "При попадании луч зелёный", line.material.GetColor("_BaseColor") == Color.green, "color=" + line.material.GetColor("_BaseColor"));

        Debug.Clear();
        Call(p, "NazhatieTriggera", new InputAction.CallbackContext());
        var planted = GameObject.All.Find(x => x.name == "Дуб(Clone)" && !x.Destroyed);
        Check(g, "Триггер сажает дерево в точке попадания", planted != null && planted.transform.position == new Vector3(0,0,10), "pos=" + (planted == null ? "нет" : planted.transform.position.ToString()));
        Check(g, "У посаженного дерева запомнен sourcePrefab", planted != null && planted.GetComponent<Nasazhdenie>() != null && planted.GetComponent<Nasazhdenie>().sourcePrefab == prefab, "source ok");
        Check(g, "В лог записана посадка", Debug.Messages.Exists(x => x.StartsWith("LOG: Посажено")), string.Join(";", Debug.Messages));

        SostoyanieMenyu.InventoryOpen = false;
        SostoyanieMenyu.ToolsMenuOpen = true;
        p.rabotatPriOtkrytomMenyu = false;
        Call(p, "LateUpdate");
        Check(g, "Открытое меню блокирует луч при настройке", !line.enabled, "line=" + line.enabled);

        var sel = Go("select", typeof(VyborNasazhdeniya)).GetComponent<VyborNasazhdeniya>();
        GameObject pg;
        var inv = MakeInventory(out pg);
        sel.inventar = inv;
        Set(sel, "luch", Go("selLine", typeof(LineRenderer)).GetComponent<LineRenderer>());
        Set(sel, "materialLuchaKopiya", new Material("selLineMat"));
        sel.materialPodsvetki = new Material("highlight");

        SostoyanieMenyu.InventoryOpen = true;
        SostoyanieMenyu.ToolsMenuOpen = false;
        var target = Plant("Липа", "Деревья", false, new Vector3(1,0,1), Vector3.one);
        Physics.Handler = (o, d, max, mask, qti) => new RaycastHit {
            point = new Vector3(1,0,1), collider = target.gameObject.AddComponent<Collider>(), distance = 1f
        };
        Call(sel, "LateUpdate");
        Check(g, "Наведение на насаждение обнаружено", Get(sel, "navedennoe") == target, "hovered=" + (Get(sel, "navedennoe") == null ? "null" : "объект"));

        Call(sel, "NazhatieTriggera", new InputAction.CallbackContext());
        Check(g, "Триггер выбирает наведённое насаждение", inv.tekushiyObyekt == target, "selected=" + (inv.tekushiyObyekt == null ? "null" : inv.tekushiyObyekt.vidNasazhdeniya));

        var terrainHit = Go("terr", typeof(Collider));
        Physics.Handler = (o, d, max, mask, qti) => new RaycastHit {
            point = new Vector3(7,0,7), collider = terrainHit.GetComponent<Collider>(), distance = 7f
        };
        Call(sel, "NazhatieGrip", new InputAction.CallbackContext());
        Call(sel, "LateUpdate");
        Check(g, "Grip приклеивает объект к точке на террейне", target.transform.position == new Vector3(7,0,7), "pos=" + target.transform.position);
        Call(sel, "OtpuskanieGrip", new InputAction.CallbackContext());
        Check(g, "Отпускание Grip прекращает перемещение", Get(sel, "peremeshchaemyi") == null, "moving=" + Get(sel, "peremeshchaemyi"));

        sel.SbrositVydelenie();
        Check(g, "Сброс выделения очищает инвентарь", inv.tekushiyObyekt == null && Get(sel, "navedennoe") == null, "object=" + inv.tekushiyObyekt);
    }

    static void UnitRotationAndMisc()
    {
        Reset();
        string g = "unit-rotation-misc";

        GameObject pg;
        var inv = MakeInventory(out pg);
        var rot = Go("rot", typeof(VrashenieDereva)).GetComponent<VrashenieDereva>();
        rot.inventar = inv;
        var action = new InputAction();
        Set(rot, "deistvieDzhoystika", action);
        rot.skorostVrascheniya = 90f;
        rot.mertvayaZona = 0.2f;
        rot.porogVyhoda = 0.6f;
        var tree = Plant("Дуб", "Деревья", false, Vector3.zero, Vector3.one);
        inv.UstanovitTekushiyObyekt(tree);
        Call(inv, "Povernut");

        float y0 = tree.transform.rotation.y;
        action.StoredValue = new Vector2(0.1f, 0);
        Call(rot, "Update");
        Check(g, "Мёртвая зона не крутит объект", Near(tree.transform.rotation.y, y0), "rot=" + tree.transform.rotation.y);

        action.StoredValue = new Vector2(0.8f, 0);
        Call(rot, "Update");
        Check(g, "Горизонтальный наклон крутит против часовой", tree.transform.rotation.y < y0, "rot=" + tree.transform.rotation.y);

        action.StoredValue = new Vector2(0, 0.9f);
        Call(rot, "Update");
        Check(g, "Вертикальный наклон выключает режим поворота", !inv.RezhimVrascheniya, "mode=" + inv.RezhimVrascheniya);

        var gate = Go("gate", typeof(PanelPereklyuchatel)).GetComponent<PanelPereklyuchatel>();
        gate.panel = Go("uiPanel");
        gate.panel.SetActive(false);
        gate.Otkryt();
        Check(g, "Otkryt показывает панель", gate.panel.activeSelf, "active=" + gate.panel.activeSelf);
        gate.Otkryt();
        Check(g, "Повторный Otkryt ничего не меняет", gate.panel.activeSelf, "active=" + gate.panel.activeSelf);
        gate.Zakryt();
        Check(g, "Zakryt скрывает панель", !gate.panel.activeSelf, "active=" + gate.panel.activeSelf);

        var cycle = Go("cycle", typeof(PereklyuchenieKnopkoy)).GetComponent<PereklyuchenieKnopkoy>();
        var m = Go("m", typeof(MenuVkladki)).GetComponent<MenuVkladki>();
        cycle.menu = m;
        m.elementyVkladok = new Selectable[] { Go("x1", typeof(Button)).GetComponent<Button>(), Go("x2", typeof(Button)).GetComponent<Button>() };
        Call(cycle, "NazhatieKnopki", new InputAction.CallbackContext());
        Check(g, "Кнопка переключения листает вкладку", m.TekushayaVkladka == 1, "tab=" + m.TekushayaVkladka);

        var gz = Go("gazon", typeof(GazonInstrumenty)).GetComponent<GazonInstrumenty>();
        gz.knopkaKist = Go("kb", typeof(Button)).GetComponent<Button>();
        gz.knopkaLastik = Go("kl", typeof(Button)).GetComponent<Button>();
        gz.panelPlotnosti = Go("plotnost");
        Call(gz, "VybratKist");
        Check(g, "Кисть показывает слайдер плотности", gz.panelPlotnosti.activeSelf, "active=" + gz.panelPlotnosti.activeSelf);
        Call(gz, "VybratLastik");
        Check(g, "Ластик скрывает слайдер плотности", !gz.panelPlotnosti.activeSelf, "active=" + gz.panelPlotnosti.activeSelf);

        var kb = Go("plantBtn", typeof(KnopkaNasazhdeniya));
        var k = kb.GetComponent<KnopkaNasazhdeniya>();
        k.prefabNasazhdeniya = Go("selPref");
        k.sbrasyvatPriSkrytii = true;
        Call(k, "Vybrat");
        Check(g, "Выбор кнопки запоминает префаб", VyborInstrumenta.TekushiyPrefab == k.prefabNasazhdeniya, "prefab=" + VyborInstrumenta.TekushiyPrefab);
        Call(k, "OnDisable");
        Check(g, "Скрытие выбранной кнопки сбрасывает выбор", VyborInstrumenta.TekushiyPrefab == null, "prefab=" + VyborInstrumenta.TekushiyPrefab);

        var under = Go("under", typeof(PodsvetkaNasazhdeniya));
        var pod = under.GetComponent<PodsvetkaNasazhdeniya>();
        var rend = under.AddComponent<Renderer>();
        rend.sharedMaterials = new[] { new Material("orig") };
        var origin = rend.sharedMaterials[0];
        Call(pod, "Awake");
        var hl = new Material("hl");
        pod.Vkluchit(hl);
        Check(g, "Подсветка меняет материал", rend.sharedMaterials[0] == hl, "mat=" + rend.sharedMaterials[0].name);
        pod.Vkluchit(hl);
        pod.Vyklyuchit();
        Check(g, "Снятие подсветки возвращает исходный материал", rend.sharedMaterials[0] == origin, "mat=" + rend.sharedMaterials[0].name);
        pod.Vyklyuchit();
        Check(g, "Повторное снятие подсветки безопасно", rend.sharedMaterials[0] == origin, "mat=" + rend.sharedMaterials[0].name);
    }

    static void Scenarios()
    {
        Reset();
        string g = "scenario";

        GameObject pg;
        var inv = MakeInventory(out pg);
        var nav = Go("nav", typeof(DzhoystikNavigatsiyaInventarya)).GetComponent<DzhoystikNavigatsiyaInventarya>();
        Set(nav, "inventar", inv);
        var oldPrefab = Go("Дуб_prefab", typeof(Nasazhdenie));
        var newPrefab = Go("Липа_prefab", typeof(Nasazhdenie));
        nav.knopkiZamenyDerevya = new[] { MakeReplaceButton("Дуб", oldPrefab), MakeReplaceButton("Липа", newPrefab) };
        nav.panelZamenyDerevya = Go("replacePanel");
        nav.knopkiZamenyKusty = new[] { MakeReplaceButton("Бук", Go("b")) };
        nav.panelZamenyKusty = Go("bushPanel");
        nav.kolonokZamenyDerevya = 2;
        nav.kolonokZamenyKusty = 1;
        SostoyanieMenyu.InventoryOpen = true;

        var controller = Go("ctrl", typeof(KontrollerZameny)).GetComponent<KontrollerZameny>();
        controller.inventar = inv;
        controller.panelZameny = Go("panel", typeof(PanelZameny)).GetComponent<PanelZameny>();

        var planted = Plant("Дуб", "Деревья", true, new Vector3(2,0,5), new Vector3(1.5f,1.5f,1.5f), oldPrefab);
        planted.transform.rotation = Quaternion.Euler(0, 0.5f, 0);
        inv.UstanovitTekushiyObyekt(planted);
        nav.OtkrytPanelZameny(planted);
        Check(g, "Сценарий 1: панель замены открыта для посаженного дерева", (bool)Get(nav, "rezhimZameny"), "rezhim=" + Get(nav, "rezhimZameny"));

        nav.PrimenitZamenu(nav.knopkiZamenyDerevya[1]);
        var newPlant = GameObject.All.Find(x => x.name == "Липа_prefab(Clone)" && !x.Destroyed);
        Check(g, "Сценарий 1: создано новое насаждение вместо старого", newPlant != null, "obj=" + (newPlant == null ? "нет" : "есть"));
        if (newPlant != null)
        {
            Check(g, "Сценарий 1: позиция и масштаб сохранены", newPlant.transform.position == new Vector3(2,0,5) && newPlant.transform.localScale == new Vector3(1.5f,1.5f,1.5f), "pos=" + newPlant.transform.position + " scale=" + newPlant.transform.localScale);
            Check(g, "Сценарий 1: старый объект удалён", planted.gameObject.Destroyed, "destroyed=" + planted.gameObject.Destroyed);
            Check(g, "Сценарий 1: источник префаба обновлён", newPlant.GetComponent<Nasazhdenie>().sourcePrefab == newPrefab, "source ok");
            Check(g, "Сценарий 1: фиксация перенесена", newPlant.GetComponent<Nasazhdenie>().zafiksirovano, "fixed=" + newPlant.GetComponent<Nasazhdenie>().zafiksirovano);
            Check(g, "Сценарий 1: инвентарь показывает новый объект", inv.tekushiyObyekt == newPlant.GetComponent<Nasazhdenie>(), "selected ok");
        }
        Check(g, "Сценарий 1: панель замены закрылась", !(bool)Get(nav, "rezhimZameny"), "rezhim=" + Get(nav, "rezhimZameny"));

        Reset();
        GameObject p2;
        var inv2 = MakeInventory(out p2);
        var ctrl2 = Go("ctrl2", typeof(KontrollerZameny)).GetComponent<KontrollerZameny>();
        ctrl2.inventar = inv2;
        ctrl2.panelZameny = Go("panel2", typeof(PanelZameny)).GetComponent<PanelZameny>();
        var same = Plant("дуб", "Деревья", false, new Vector3(3,0,3), Vector3.one);
        inv2.UstanovitTekushiyObyekt(same);
        Debug.Clear();
        Call(ctrl2, "ObrabotatVybor", Vid("ДУБ", Go("pref")));
        Check(g, "Сценарий 2: выбор того же вида не создаёт объект", GameObject.All.FindAll(x => x.name.EndsWith("(Clone)")).Count == 0, "clones=" + GameObject.All.FindAll(x => x.name.EndsWith("(Clone)")).Count);
        Check(g, "Сценарий 2: объект остался на месте", inv2.tekushiyObyekt == same && !same.gameObject.Destroyed, "same ok");

        Reset();
        GameObject p3;
        var inv3 = MakeInventory(out p3);
        var ctrl3 = Go("ctrl3", typeof(KontrollerZameny)).GetComponent<KontrollerZameny>();
        ctrl3.inventar = inv3;
        var panel3 = Go("panel3", typeof(PanelZameny)).GetComponent<PanelZameny>();
        ctrl3.panelZameny = panel3;
        Set(panel3, "yacheyki", new[] { Cell("s3a"), Cell("s3b") });
        ctrl3.zakryvatPosleZameny = false;
        var start3 = Plant("Дуб", "Деревья", false, Vector3.zero, Vector3.one);
        inv3.UstanovitTekushiyObyekt(start3);
        Call(ctrl3, "ObrabotatVybor", Vid("Липа", Go("Липа_prefab")));
        Check(g, "Сценарий 3: контроллер выполнил замену", start3.gameObject.Destroyed && inv3.tekushiyObyekt != null && inv3.tekushiyObyekt.vidNasazhdeniya == "Липа", "vid=" + (inv3.tekushiyObyekt == null ? "null" : inv3.tekushiyObyekt.vidNasazhdeniya));
        Check(g, "Сценарий 3: панель обновлена для нового объекта", Get(panel3, "tekushiyObyekt") == inv3.tekushiyObyekt, "panel updated");

        Reset();
        var nav4 = Go("nav4", typeof(DzhoystikNavigatsiyaInventarya)).GetComponent<DzhoystikNavigatsiyaInventarya>();
        var rotate = Go("rotCmp", typeof(BehaviourStub)).GetComponent<BehaviourStub>();
        var move = Go("moveCmp", typeof(BehaviourStub)).GetComponent<BehaviourStub>();
        Set(nav4, "komponentPovorota", rotate);
        Set(nav4, "komponentPeredvizheniya", move);
        SostoyanieMenyu.InventoryOpen = true;
        Call(nav4, "LateUpdate");
        Check(g, "Сценарий 4: при открытом инвентаре локомоция выключена", !rotate.enabled && !move.enabled, "rot=" + rotate.enabled + " move=" + move.enabled);
        SostoyanieMenyu.InventoryOpen = false;
        SostoyanieMenyu.ToolsMenuOpen = false;
        Call(nav4, "LateUpdate");
        Check(g, "Сценарий 4: после закрытия меню локомоция включена", rotate.enabled && move.enabled, "rot=" + rotate.enabled + " move=" + move.enabled);

        Reset();
        var menuNav = Go("menuNav", typeof(DzhoystikNavigatsiyaMenu)).GetComponent<DzhoystikNavigatsiyaMenu>();
        var cats = Go("cats", typeof(MenuVkladki)).GetComponent<MenuVkladki>();
        var trees = Go("trees", typeof(MenuVkladki)).GetComponent<MenuVkladki>();
        var bushes = Go("bushes", typeof(MenuVkladki)).GetComponent<MenuVkladki>();
        cats.elementyVkladok = new Selectable[] { Go("c1", typeof(Button)).GetComponent<Button>(), Go("c2", typeof(Button)).GetComponent<Button>() };
        cats.paneliVkladok = new GameObject[0];
        trees.elementyVkladok = new Selectable[] { Go("t1", typeof(Button)).GetComponent<Button>(), Go("t2", typeof(Button)).GetComponent<Button>() };
        trees.paneliVkladok = new GameObject[0];
        bushes.elementyVkladok = new Selectable[] { Go("b1", typeof(Button)).GetComponent<Button>() };
        bushes.paneliVkladok = new GameObject[0];
        menuNav.menuKategoriy = cats;
        menuNav.menuVidovPoKategoriyam = new[] { trees, bushes };
        menuNav.panelMenu = Go("toolsPanel");
        menuNav.panelMenu.SetActive(true);
        menuNav.porogNaklona = 0.5f;
        Call(menuNav, "Update");
        Check(g, "Сценарий 5: меню инструментов сообщает о своём состоянии", SostoyanieMenyu.ToolsMenuOpen, "tools=" + SostoyanieMenyu.ToolsMenuOpen);

        Call(menuNav, "SobytieDzhoystika", new InputAction.CallbackContext { value = new Vector2(1, 0) });
        Check(g, "Сценарий 5: свайп вправо входит в категорию", (bool)Get(menuNav, "vnutriKategorii"), "inside=" + Get(menuNav, "vnutriKategorii"));
        Call(menuNav, "SobytieDzhoystikaSbros", new InputAction.CallbackContext { value = Vector2.zero });
        Call(menuNav, "SobytieDzhoystika", new InputAction.CallbackContext { value = new Vector2(0, 1) });
        Check(g, "Сценарий 5: свайп вверх листает виды", TekushayaVкладкаGuard(trees) == 1, "tab=" + TekushayaVкладкаGuard(trees));
        Call(menuNav, "SobytieDzhoystikaSbros", new InputAction.CallbackContext { value = Vector2.zero });
        Call(menuNav, "SobytieDzhoystika", new InputAction.CallbackContext { value = new Vector2(-1, 0) });
        Check(g, "Сценарий 5: левый свайп возвращает к категориям", !(bool)Get(menuNav, "vnutriKategorii"), "inside=" + Get(menuNav, "vnutriKategorii"));

        Reset();
        var nav6 = Go("nav6", typeof(DzhoystikNavigatsiyaInventarya)).GetComponent<DzhoystikNavigatsiyaInventarya>();
        Debug.Clear();
        nav6.OtkrytPanelZameny(null);
        Check(g, "Сценарий 6: пустой объект -> warning без исключения", Debug.Messages.Exists(x => x.StartsWith("WARN")), string.Join(";", Debug.Messages));
        Debug.Clear();
        var badCat = Plant("Газон", "Газон", false, Vector3.zero, Vector3.one);
        nav6.OtkrytPanelZameny(badCat);
        Check(g, "Сценарий 6: неподдерживаемая категория -> warning", Debug.Messages.Exists(x => x.Contains("не настроена")), string.Join(";", Debug.Messages));
        Debug.Clear();
        var panel6 = Go("p6", typeof(PanelZameny)).GetComponent<PanelZameny>();
        panel6.katalog = new KategoriyaZameny[0];
        Set(panel6, "pokazannyeVidy", new VidZameny[0]);
        Set(panel6, "yacheyki", new[] { Cell("x") });
        var obj6 = Plant("Дуб", "Деревья", false, Vector3.zero, Vector3.one);
        Set(panel6, "tekushiyObyekt", obj6);
        Call(panel6, "PokazatVidy", true);
        Check(g, "Сценарий 6: отсутствующая категория -> пустой каталог и warning", Debug.Messages.Exists(x => x.Contains("нет категории")), string.Join(";", Debug.Messages));

        Reset();
        var pos7 = Go("pos7", typeof(PosadkaDereva)).GetComponent<PosadkaDereva>();
        Set(pos7, "luch", Go("l7", typeof(LineRenderer)).GetComponent<LineRenderer>());
        Set(pos7, "materialLuchaKopiya", new Material("m7"));
        pos7.materialPrizraka = new Material("ghost7");
        var prefA = Go("A", typeof(Nasazhdenie));
        var prefB = Go("B", typeof(Nasazhdenie));
        Physics.Handler = (o, d, max, mask, qti) => new RaycastHit { point = new Vector3(0,0,5), collider = Go("t7", typeof(Collider)).GetComponent<Collider>(), distance = 5 };
        VyborInstrumenta.TekushiyPrefab = prefA;
        Call(pos7, "LateUpdate");
        var ghostA = (GameObject)Get(pos7, "prizrak");
        VyborInstrumenta.TekushiyPrefab = prefB;
        Call(pos7, "LateUpdate");
        var ghostB = (GameObject)Get(pos7, "prizrak");
        Check(g, "Сценарий 7: призрак пересоздан при смене префаба", ghostA != null && ghostB != null && ghostA != ghostB && ghostA.Destroyed, "ghosts different");
        Call(pos7, "OnDisable");
        Check(g, "Сценарий 7: при выключении луч и призрак скрыты", !((LineRenderer)Get(pos7, "luch")).enabled && !((GameObject)Get(pos7, "prizrak")).activeSelf, "cleaned");

        Reset();
        GameObject p8;
        var inv8 = MakeInventory(out p8);
        var sel8 = Go("sel8", typeof(VyborNasazhdeniya)).GetComponent<VyborNasazhdeniya>();
        sel8.inventar = inv8;
        Set(sel8, "luch", Go("l8", typeof(LineRenderer)).GetComponent<LineRenderer>());
        Set(sel8, "materialLuchaKopiya", new Material("m8"));
        sel8.materialPodsvetki = new Material("hl8");
        SostoyanieMenyu.InventoryOpen = true;
        var target8 = Plant("Дуб", "Деревья", false, Vector3.zero, Vector3.one);
        Physics.Handler = (o, d, max, mask, qti) => new RaycastHit { point = Vector3.zero, collider = target8.gameObject.AddComponent<Collider>(), distance = 1 };
        Call(sel8, "LateUpdate");
        Call(sel8, "NazhatieTriggera", new InputAction.CallbackContext());
        Check(g, "Сценарий 8: объект выбран и подсвечен", inv8.tekushiyObyekt == target8, "selected ok");
        Call(inv8, "Udalit");
        Check(g, "Сценарий 8: удаление очищает выбор инвентаря", inv8.tekushiyObyekt == null, "selected=" + inv8.tekushiyObyekt);
        Check(g, "Сценарий 8: объект помечен на удаление", target8.gameObject.Destroyed, "destroyed=" + target8.gameObject.Destroyed);

        Reset();
        var nav9 = Go("nav9", typeof(DzhoystikNavigatsiyaInventarya)).GetComponent<DzhoystikNavigatsiyaInventarya>();
        var replBtn = Go("replaceBtn", typeof(Button)).GetComponent<Button>();
        nav9.knopkaZamenitVid = replBtn;
        nav9.elementyMenu = new Selectable[] { Go("z1", typeof(Button)).GetComponent<Button>(), replBtn };
        nav9.panelMenu = Go("p9");
        nav9.panelMenu.SetActive(true);
        nav9.knopkiZamenyDerevya = new[] { MakeReplaceButton("Дуб", Go("d")) };
        nav9.panelZamenyDerevya = Go("rp9");
        nav9.knopkiZamenyKusty = new[] { MakeReplaceButton("Бук", Go("b")) };
        nav9.panelZamenyKusty = Go("bp9");
        var obj9 = Plant("Дуб", "Деревья", false, Vector3.zero, Vector3.one);
        nav9.OtkrytPanelZameny(obj9);
        nav9.ZakrytPanelZameny();
        Check(g, "Сценарий 9: фокус вернулся на кнопку замены", EventSystem.current.currentSelectedGameObject == replBtn.gameObject, "selected=" + (EventSystem.current.currentSelectedGameObject == null ? "null" : EventSystem.current.currentSelectedGameObject.name));

        Reset();
        GameObject p10;
        var inv10 = MakeInventory(out p10);
        inv10.slaiderRazmer = (Slider)Get(inv10, "slaiderRazmer");
        inv10.shagRastyagivaniya = 0.2f;
        var bed = Plant("Клумба", "Клумбы", false, Vector3.zero, Vector3.one);
        inv10.UstanovitTekushiyObyekt(bed);
        inv10.slaiderRazmer.SetValueWithoutNotify(0.5f);
        Call(inv10, "IzmenitRazmer", 0.5f);
        Call(inv10, "Rastyanut");
        Check(g, "Сценарий 10: размер и растягивание применяются вместе", Near(bed.transform.localScale.x, 1f) && Near(bed.transform.localScale.z, 1.2f), "scale=" + bed.transform.localScale);
    }
}

class BehaviourStub : MonoBehaviour {}
