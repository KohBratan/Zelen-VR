using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Object = UnityEngine.Object;

// Однократный редакторский генератор сцен меню. Не вызывает сборку проекта.
public static class ZelenMenuBuilder
{
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/Others/Supercyan Free Forest Sample/Scenes/BrightDay.unity";
    private const string GamerPath = "Assets/Prefabs/Gamer.prefab";
    private const string BackgroundPath = "Assets/UI/StartMenuBackground.png";
    private const string InputActionsPath = "Assets/Others/Samples/XR Interaction Toolkit/3.1.2/Starter Assets/XRI Default Input Actions.inputactions";

    private const string MenuFontPath = "Assets/UI/InterCyrillic SDF.asset";
    private static TMP_FontAsset font;
    private static Sprite uiSprite;

    [MenuItem("Zelen/Создать или обновить меню")]
    public static void SozdatMenyu()
    {
        ZagruzitShrift();
        uiSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        Directory.CreateDirectory("Assets/Scenes");
        AssetDatabase.ImportAsset(BackgroundPath, ImportAssetOptions.ForceSynchronousImport);

        SozdatGlavnoeMenu();
        DobavitIgrovoeMenu();
        NastroitBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Zelen: стартовая сцена и игровое меню созданы. Билд не запускался.");
    }

    private static void ZagruzitShrift()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MenuFontPath);
        if (font == null)
        {
            Font source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Others/VRTemplateAssets/Fonts/Inter/Inter-Regular.ttf");
            if (source == null)
            {
                throw new FileNotFoundException("Не найден исходный шрифт Inter-Regular.ttf");
            }

            font = TMP_FontAsset.CreateFontAsset(source);
            font.name = "InterCyrillic SDF";
            AssetDatabase.CreateAsset(font, MenuFontPath);
            font.material.name = "InterCyrillic SDF Material";
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (Texture2D atlas in font.atlasTextures)
            {
                atlas.name = "InterCyrillic SDF Atlas";
                AssetDatabase.AddObjectToAsset(atlas, font);
            }
        }

        string characters = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя—–·«»";
        for (char character = ' '; character <= '~'; character++)
        {
            characters += character;
        }
        if (!font.TryAddCharacters(characters, out string missing))
        {
            throw new InvalidOperationException("В шрифте Inter отсутствуют символы: " + missing);
        }
        EditorUtility.SetDirty(font);
    }

    private static void SozdatGlavnoeMenu()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject gamerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(GamerPath);
        GameObject gamer = PrefabUtility.InstantiatePrefab(gamerAsset) as GameObject;
        gamer.name = "Gamer (Main Menu)";
        gamer.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        Camera camera = gamer.GetComponentsInChildren<Camera>(true).FirstOrDefault();
        Canvas canvas = SozdatCanvas("MainMenuCanvas", new Vector2(1920f, 1080f), 0.00145f, true, camera);
        canvas.transform.position = new Vector3(0f, 1.6f, 2.4f);

        RawImage background = SozdatFon(canvas.transform, "Background");
        background.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
        background.color = Color.white;

        SozdatPanel(canvas.transform, "Darkening", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0.015f, 0.035f, 0.03f, 0.28f));
        SozdatPanel(canvas.transform, "LeftShade", new Vector2(0f, 0f), new Vector2(0.52f, 1f),
            Vector2.zero, Vector2.zero, new Color(0.015f, 0.04f, 0.033f, 0.88f));
        SozdatPanel(canvas.transform, "AccentLine", new Vector2(0.09f, 0.21f), new Vector2(0.095f, 0.77f),
            Vector2.zero, Vector2.zero, Cvet("78C850"));

        TMP_Text title = SozdatText(canvas.transform, "Title", "ЗЕЛЁНЫЕ\nНАСАЖДЕНИЯ", 82f, FontStyles.Bold,
            TextAlignmentOptions.Left, Color.white);
        UstanovitRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(160f, -150f), new Vector2(850f, 250f));

        TMP_Text subtitle = SozdatText(canvas.transform, "Subtitle", "VR-модуль озеленения территории", 32f,
            FontStyles.Normal, TextAlignmentOptions.Left, Cvet("D5E8DC"));
        UstanovitRect(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(165f, -310f), new Vector2(760f, 70f));

        TMP_Text project = SozdatText(canvas.transform, "Project", "Виртуальный градостроитель · режим великана", 22f,
            FontStyles.Normal, TextAlignmentOptions.Left, Cvet("A8BCAF"));
        UstanovitRect(project.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(0f, 0f), new Vector2(165f, 95f), new Vector2(820f, 55f));

        Button play = SozdatKnopka(canvas.transform, "PlayButton", "Играть", new Vector2(340f, -420f),
            new Vector2(470f, 105f), Cvet("4E9B52"), 38f);
        Button quit = SozdatKnopka(canvas.transform, "QuitButton", "Выйти", new Vector2(340f, -550f),
            new Vector2(470f, 95f), Cvet("263D35"), 34f);

        GameObject manager = new GameObject("MainMenuManager");
        GlavnoeMenu menu = manager.AddComponent<GlavnoeMenu>();
        menu.knopkaIgrat = play;
        menu.knopkaVyyti = quit;
        menu.kamera = camera != null ? camera.transform : null;
        menu.panel = canvas.transform;
        menu.imyaIgrovoiSceny = "BrightDay";

        List<Behaviour> disabled = gamer.GetComponentsInChildren<Behaviour>(true)
            .Where(component => component is PosadkaDereva || component is VyborNasazhdeniya ||
                                component is PanelPereklyuchatel || component is DzhoystikNavigatsiyaMenu ||
                                component is DzhoystikNavigatsiyaInventarya || component is VrashenieDereva ||
                                component is PereklyuchenieKnopkoy || component is LocomotionProvider)
            .ToList();
        menu.otklyuchaemyeKomponenty = disabled.ToArray();
        menu.skryvaemyePaneli = gamer.GetComponentsInChildren<PanelPereklyuchatel>(true)
            .Where(component => component.panel != null).Select(component => component.panel).ToArray();

        ObespechitEventSystem();

        GameObject lightObject = new GameObject("MenuLight");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.8f;
        lightObject.transform.rotation = Quaternion.Euler(35f, -25f, 0f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.22f, 0.27f, 0.24f);

        EditorSceneManager.SaveScene(scene, MainMenuPath);
    }

    private static void DobavitIgrovoeMenu()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        IgrovoeMenu existing = Object.FindObjectsOfType<IgrovoeMenu>(true).FirstOrDefault();
        if (existing != null)
        {
            // Удаляем только ранее созданные нами объекты, чтобы генератор был идемпотентным.
            if (existing.panelMenu != null)
            {
                Object.DestroyImmediate(existing.panelMenu);
            }
            if (existing.panelKratkihPodskazok != null)
            {
                Object.DestroyImmediate(existing.panelKratkihPodskazok);
            }
            Object.DestroyImmediate(existing.gameObject);
        }

        Camera camera = Object.FindObjectsOfType<Camera>(true).FirstOrDefault(item => item.CompareTag("MainCamera"));
        if (camera == null)
        {
            camera = Object.FindObjectsOfType<Camera>(true).FirstOrDefault();
        }

        Canvas menuCanvas = SozdatCanvas("GameMenuCanvas", new Vector2(1200f, 1000f), 0.00135f, true, camera);
        SozdatPanel(menuCanvas.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0.025f, 0.055f, 0.047f, 0.97f));
        SozdatPanel(menuCanvas.transform, "Accent", new Vector2(0f, 0.88f), new Vector2(1f, 1f),
            Vector2.zero, Vector2.zero, Cvet("355F45"));

        TMP_Text title = SozdatText(menuCanvas.transform, "Title", "Меню", 62f, FontStyles.Bold,
            TextAlignmentOptions.Center, Color.white);
        UstanovitRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -82f), new Vector2(700f, 100f));

        Button resume = SozdatKnopka(menuCanvas.transform, "Resume", "Продолжить", new Vector2(0f, 170f),
            new Vector2(760f, 100f), Cvet("4E9B52"), 34f);
        Button info = SozdatKnopka(menuCanvas.transform, "Information", "Информация и управление", new Vector2(0f, 45f),
            new Vector2(760f, 100f), Cvet("2E4C40"), 32f);
        Button hints = SozdatKnopka(menuCanvas.transform, "Hints", "Краткие подсказки: вкл. / выкл.", new Vector2(0f, -80f),
            new Vector2(760f, 100f), Cvet("2E4C40"), 30f);
        Button mainMenu = SozdatKnopka(menuCanvas.transform, "MainMenu", "В главное меню", new Vector2(0f, -205f),
            new Vector2(760f, 100f), Cvet("2E4C40"), 32f);
        Button quit = SozdatKnopka(menuCanvas.transform, "Quit", "Выйти из игры", new Vector2(0f, -330f),
            new Vector2(760f, 100f), Cvet("6D3939"), 32f);

        GameObject infoPanel = SozdatPanel(menuCanvas.transform, "InformationPanel", Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, new Color(0.025f, 0.055f, 0.047f, 0.995f)).gameObject;
        infoPanel.GetComponent<Image>().raycastTarget = true;
        TMP_Text infoTitle = SozdatText(infoPanel.transform, "InformationTitle", "Управление", 54f, FontStyles.Bold,
            TextAlignmentOptions.Center, Cvet("B8E3B5"));
        UstanovitRect(infoTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(850f, 90f));

        const string instruction =
            "ЛЕВЫЙ КОНТРОЛЛЕР\n" +
            "Стик — перемещение по территории\n" +
            "Y — открыть / закрыть меню инструментов\n" +
            "Триггер — посадить выбранное растение\n" +
            "Стик в меню — навигация; клик стика — выбрать\n" +
            "Menu — открыть / закрыть игровое меню\n\n" +
            "ПРАВЫЙ КОНТРОЛЛЕР\n" +
            "Стик — поворот камеры; навигация в инвентаре\n" +
            "B — открыть / закрыть инвентарь\n" +
            "Триггер — выбрать растение под лучом\n" +
            "Grip — перемещать выбранное растение по террейну\n" +
            "Клик стика — подтвердить действие в инвентаре\n\n" +
            "Выбор кнопок во всех меню — лучом и триггером.";
        TMP_Text infoText = SozdatText(infoPanel.transform, "InformationText", instruction, 26f, FontStyles.Normal,
            TextAlignmentOptions.Left, Color.white);
        UstanovitRect(infoText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1020f, 650f));
        Button back = SozdatKnopka(infoPanel.transform, "Back", "Назад", new Vector2(0f, -410f),
            new Vector2(420f, 90f), Cvet("4E9B52"), 30f);
        infoPanel.SetActive(false);

        Canvas quickCanvas = SozdatCanvas("QuickHintsCanvas", new Vector2(1050f, 330f), 0.00072f, false, camera);
        SozdatPanel(quickCanvas.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0.02f, 0.045f, 0.038f, 0.9f));
        TMP_Text quickText = SozdatText(quickCanvas.transform, "HintsText",
            "Y — инструменты    B — инвентарь    Menu — меню\n" +
            "Левый триггер — посадить    Правый триггер — выбрать\n" +
            "Правый Grip — переместить    Стики — движение и поворот",
            27f, FontStyles.Normal, TextAlignmentOptions.Center, Color.white);
        Rastyanut(quickText.rectTransform, Vector2.zero, Vector2.one, new Vector2(25f, 18f), new Vector2(-25f, -18f));
        quickCanvas.gameObject.SetActive(false);

        GameObject manager = new GameObject("GameMenuManager");
        IgrovoeMenu menu = manager.AddComponent<IgrovoeMenu>();
        menu.panelMenu = menuCanvas.gameObject;
        menu.panelInformacii = infoPanel;
        menu.panelKratkihPodskazok = quickCanvas.gameObject;
        menu.kamera = camera != null ? camera.transform : null;
        menu.zakryvaemyePaneli = Object.FindObjectsOfType<PanelPereklyuchatel>(true);
        menu.knopkaProdolzhit = resume;
        menu.knopkaInformaciya = info;
        menu.knopkaPodskazki = hints;
        menu.knopkaGlavnoeMenu = mainMenu;
        menu.knopkaVyyti = quit;
        menu.knopkaNazadOtInformacii = back;
        menu.imyaScenyGlavnogoMenyu = "MainMenu";

        PosadkaDereva planting = Object.FindObjectsOfType<PosadkaDereva>(true).FirstOrDefault();
        menu.naborDeistvii = planting != null
            ? planting.naborDeistvii
            : AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(InputActionsPath);

        menuCanvas.gameObject.SetActive(false);
        ObespechitEventSystem();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, GameScenePath);
    }

    private static void NastroitBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MainMenuPath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.path != MainMenuPath && scene.path != GameScenePath && !scenes.Any(item => item.path == scene.path))
            {
                scenes.Add(scene);
            }
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void ObespechitEventSystem()
    {
        EventSystem eventSystem = Object.FindObjectsOfType<EventSystem>(true).FirstOrDefault();
        if (eventSystem == null)
        {
            GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(XRUIInputModule));
            eventSystem = go.GetComponent<EventSystem>();
        }
        if (eventSystem.GetComponent<XRUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<XRUIInputModule>();
        }
    }

    private static Canvas SozdatCanvas(string name, Vector2 size, float scale, bool interactable, Camera camera)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        canvas.pixelPerfect = false;

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.localScale = Vector3.one * scale;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 4f;
        if (interactable)
        {
            go.AddComponent<TrackedDeviceGraphicRaycaster>();
        }
        return canvas;
    }

    private static RawImage SozdatFon(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(parent, false);
        RawImage image = go.GetComponent<RawImage>();
        image.raycastTarget = false;
        Rastyanut(go.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return image;
    }

    private static Image SozdatPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        if (uiSprite != null)
        {
            image.sprite = uiSprite;
            image.type = Image.Type.Sliced;
        }
        Rastyanut(go.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
        return image;
    }

    private static TMP_Text SozdatText(Transform parent, string name, string text, float size, FontStyles style,
        TextAlignmentOptions alignment, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;
        label.overflowMode = TextOverflowModes.Overflow;
        return label;
    }

    private static Button SozdatKnopka(Transform parent, string name, string text, Vector2 position, Vector2 size,
        Color color, float fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        UstanovitRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

        Image image = go.GetComponent<Image>();
        image.color = color;
        if (uiSprite != null)
        {
            image.sprite = uiSprite;
            image.type = Image.Type.Sliced;
        }

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(0.78f, 0.88f, 0.8f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
        button.colors = colors;

        TMP_Text label = SozdatText(go.transform, "Label", text, fontSize, FontStyles.Bold,
            TextAlignmentOptions.Center, Color.white);
        Rastyanut(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(16f, 8f), new Vector2(-16f, -8f));
        return button;
    }

    private static void UstanovitRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void Rastyanut(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static Color Cvet(string html)
    {
        ColorUtility.TryParseHtmlString("#" + html, out Color color);
        return color;
    }
}
