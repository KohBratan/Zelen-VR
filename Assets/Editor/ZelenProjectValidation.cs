using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Object = UnityEngine.Object;

// Редакторская проверка без запуска Play Mode и без создания билда.
public static class ZelenProjectValidation
{
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/Others/Supercyan Free Forest Sample/Scenes/BrightDay.unity";
    private const string InputActionsPath = "Assets/Others/Samples/XR Interaction Toolkit/3.1.2/Starter Assets/XRI Default Input Actions.inputactions";
    private static int failures;

    [MenuItem("Zelen/Проверить меню и механики")]
    public static void Proverit()
    {
        failures = 0;
        ProveritBuildSettings();
        ProveritVvod();
        ProveritGlavnoeMenu();
        ProveritIgrovoeMenu();
        ProveritPosadku();

        if (failures > 0)
        {
            Debug.LogError("Zelen validation: обнаружено ошибок: " + failures);
            EditorApplication.Exit(2);
            return;
        }

        Debug.Log("Zelen validation: все редакторские проверки пройдены. Билд не запускался.");
    }

    private static void ProveritBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        ProveritUslovie(scenes.Length >= 2, "В Build Settings есть минимум две сцены");
        if (scenes.Length < 2)
        {
            return;
        }

        ProveritUslovie(scenes[0].enabled && scenes[0].path == MainMenuPath,
            "MainMenu — первая включённая сцена в Build Settings");
        ProveritUslovie(scenes[1].enabled && scenes[1].path == GameScenePath,
            "BrightDay — вторая включённая сцена в Build Settings");
    }

    private static void ProveritVvod()
    {
        InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
        ProveritSsylku(asset, "XRI Default Input Actions");
        if (asset == null)
        {
            return;
        }

        InputAction openMenu = asset.FindAction("XRI Left Interaction/OpenGameMenu");
        ProveritUslovie(openMenu != null, "Действие OpenGameMenu найдено");
        if (openMenu != null)
        {
            ProveritPrivyazku(openMenu, "<XRController>{LeftHand}/{MenuButton}");
            ProveritPrivyazku(openMenu, "<XRSimulatedController>{LeftHand}/menuButton");
            ProveritPrivyazku(openMenu, "<XRController>{RightHand}/{MenuButton}");
            ProveritPrivyazku(openMenu, "<XRSimulatedController>{RightHand}/menuButton");
        }

        ProveritTrigger(asset, "XRI Left Interaction/PlaceTool", "LeftHand");
        ProveritTrigger(asset, "XRI Right Interaction/PickTool", "RightHand");
    }

    private static void ProveritTrigger(InputActionAsset asset, string actionPath, string hand)
    {
        InputAction action = asset.FindAction(actionPath);
        ProveritUslovie(action != null, actionPath + " найдено");
        if (action == null)
        {
            return;
        }

        bool triggerFound = action.bindings.Any(binding =>
            !binding.isComposite && !binding.isPartOfComposite &&
            binding.effectivePath.IndexOf(hand, StringComparison.OrdinalIgnoreCase) >= 0 &&
            binding.effectivePath.IndexOf("trigger", StringComparison.OrdinalIgnoreCase) >= 0);
        ProveritUslovie(triggerFound, actionPath + " привязано к триггеру " + hand);
    }

    private static void ProveritPrivyazku(InputAction action, string expectedPath)
    {
        ProveritUslovie(action.bindings.Any(binding => binding.effectivePath == expectedPath),
            action.name + " имеет привязку " + expectedPath);
    }

    private static void ProveritGlavnoeMenu()
    {
        EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
        GlavnoeMenu menu = Object.FindObjectsOfType<GlavnoeMenu>(true).FirstOrDefault();
        ProveritSsylku(menu, "GlavnoeMenu в MainMenu");
        if (menu == null)
        {
            return;
        }

        ProveritSsylku(menu.knopkaIgrat, "Кнопка Играть");
        ProveritSsylku(menu.knopkaVyyti, "Кнопка Выйти");
        ProveritSsylku(menu.kamera, "Камера главного меню");
        ProveritSsylku(menu.panel, "Панель главного меню");
        ProveritUslovie(menu.imyaIgrovoiSceny == "BrightDay", "Главное меню открывает BrightDay");

        Canvas canvas = menu.panel != null ? menu.panel.GetComponent<Canvas>() : null;
        ProveritInteraktivnyCanvas(canvas, "MainMenuCanvas");
        ProveritEventSystem("MainMenu");
        ProveritOtsutstvieMissingScripts(SceneManager.GetActiveScene());
        ProveritShrift();
    }

    private static void ProveritIgrovoeMenu()
    {
        EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        IgrovoeMenu menu = Object.FindObjectsOfType<IgrovoeMenu>(true).FirstOrDefault();
        ProveritSsylku(menu, "IgrovoeMenu в BrightDay");
        if (menu == null)
        {
            return;
        }

        ProveritSsylku(menu.panelMenu, "Игровое меню");
        ProveritSsylku(menu.panelInformacii, "Панель информации");
        ProveritSsylku(menu.panelKratkihPodskazok, "Панель кратких подсказок");
        ProveritSsylku(menu.kamera, "Камера игрового меню");
        ProveritSsylku(menu.naborDeistvii, "InputActionAsset игрового меню");
        ProveritSsylku(menu.knopkaProdolzhit, "Кнопка Продолжить");
        ProveritSsylku(menu.knopkaInformaciya, "Кнопка Информация");
        ProveritSsylku(menu.knopkaPodskazki, "Кнопка подсказок");
        ProveritSsylku(menu.knopkaGlavnoeMenu, "Кнопка В главное меню");
        ProveritSsylku(menu.knopkaVyyti, "Кнопка Выйти из игры");
        ProveritSsylku(menu.knopkaNazadOtInformacii, "Кнопка Назад");

        if (menu.panelMenu != null)
        {
            ProveritUslovie(!menu.panelMenu.activeSelf, "Игровое меню изначально скрыто");
            ProveritInteraktivnyCanvas(menu.panelMenu.GetComponent<Canvas>(), "GameMenuCanvas");
        }
        if (menu.panelInformacii != null)
        {
            ProveritUslovie(!menu.panelInformacii.activeSelf, "Информация изначально скрыта");
            Image blocker = menu.panelInformacii.GetComponent<Image>();
            ProveritUslovie(blocker != null && blocker.raycastTarget,
                "Панель информации блокирует нажатия сквозь неё");
            ProveritRazmetkuInformacii(menu);
        }
        if (menu.panelKratkihPodskazok != null)
        {
            ProveritUslovie(!menu.panelKratkihPodskazok.activeSelf, "Краткие подсказки изначально скрыты");
        }

        ProveritEventSystem("BrightDay");
        ProveritOtsutstvieMissingScripts(SceneManager.GetActiveScene());
    }

    private static void ProveritRazmetkuInformacii(IgrovoeMenu menu)
    {
        TMP_Text text = menu.panelInformacii.transform.Find("InformationText")?.GetComponent<TMP_Text>();
        ProveritSsylku(text, "Текст подробной инструкции");
        if (text == null || menu.knopkaNazadOtInformacii == null)
        {
            return;
        }

        Bounds textBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            menu.panelInformacii.transform, text.rectTransform);
        Bounds backBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            menu.panelInformacii.transform, menu.knopkaNazadOtInformacii.transform);
        ProveritUslovie(textBounds.min.y >= backBounds.max.y,
            "Подробная инструкция не перекрывает кнопку Назад");
    }

    private static void ProveritInteraktivnyCanvas(Canvas canvas, string name)
    {
        ProveritSsylku(canvas, name);
        if (canvas == null)
        {
            return;
        }

        ProveritUslovie(canvas.renderMode == RenderMode.WorldSpace, name + " использует World Space");
        ProveritUslovie(canvas.GetComponent<TrackedDeviceGraphicRaycaster>() != null,
            name + " имеет TrackedDeviceGraphicRaycaster");
    }

    private static void ProveritEventSystem(string sceneName)
    {
        EventSystem[] eventSystems = Object.FindObjectsOfType<EventSystem>(true)
            .Where(item => item.isActiveAndEnabled).ToArray();
        ProveritUslovie(eventSystems.Length == 1, sceneName + ": ровно один активный EventSystem");

        BaseInputModule[] modules = eventSystems.SelectMany(item => item.GetComponents<BaseInputModule>())
            .Where(item => item.isActiveAndEnabled).ToArray();
        ProveritUslovie(modules.Length == 1 && modules[0] is XRUIInputModule,
            sceneName + ": активен только XRUIInputModule");
    }

    private static void ProveritOtsutstvieMissingScripts(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
                ProveritUslovie(count == 0, "Нет Missing Script: " + PolnyiPut(child));
            }
        }
    }

    private static string PolnyiPut(Transform item)
    {
        string path = item.name;
        while (item.parent != null)
        {
            item = item.parent;
            path = item.name + "/" + path;
        }
        return path;
    }

    private static void ProveritShrift()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/UI/InterCyrillic SDF.asset");
        ProveritSsylku(font, "Шрифт Inter");
        if (font != null)
        {
            ProveritUslovie(font.HasCharacters("Зелёные насаждения Информация Выйти"),
                "Шрифт Inter содержит нужные русские буквы");
        }
    }

    private static void ProveritPosadku()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        try
        {
            cube.name = "ZelenValidationCube";
            cube.transform.localScale = new Vector3(2f, 4f, 2f);
            cube.transform.rotation = Quaternion.Euler(25f, 40f, 15f);
            Vector3 surface = new Vector3(10f, 3f, 10f);

            PosadkaDereva.PostavitVertikalno(cube, surface);
            Renderer renderer = cube.GetComponent<Renderer>();
            ProveritUslovie(Mathf.Abs(renderer.bounds.min.y - surface.y) < 0.0001f,
                "Нижняя граница объекта совпадает с поверхностью при pivot в центре");
            ProveritUslovie(Mathf.DeltaAngle(cube.transform.eulerAngles.x, 0f) < 0.0001f &&
                            Mathf.DeltaAngle(cube.transform.eulerAngles.z, 0f) < 0.0001f,
                "Посадка обнуляет наклоны по X и Z");
            ProveritUslovie(Mathf.DeltaAngle(cube.transform.eulerAngles.y, 40f) < 0.0001f,
                "Посадка сохраняет поворот по Y");

            Quaternion rotation = cube.transform.rotation;
            Vector3 scale = cube.transform.localScale;
            Vector3 newSurface = new Vector3(20f, 5f, 20f);
            PosadkaDereva.Prizemlit(cube, newSurface);
            ProveritUslovie(cube.transform.rotation == rotation && cube.transform.localScale == scale,
                "Перемещение сохраняет rotation и scale");
            ProveritUslovie(Mathf.Abs(renderer.bounds.min.y - newSurface.y) < 0.0001f,
                "Перемещение не отрывает объект от новой поверхности");
        }
        finally
        {
            Object.DestroyImmediate(cube);
        }
    }

    private static void ProveritSsylku(Object value, string name)
    {
        ProveritUslovie(value != null, name + " назначено");
    }

    private static void ProveritUslovie(bool condition, string description)
    {
        if (condition)
        {
            Debug.Log("Zelen validation OK: " + description);
        }
        else
        {
            failures++;
            Debug.LogError("Zelen validation FAIL: " + description);
        }
    }
}
