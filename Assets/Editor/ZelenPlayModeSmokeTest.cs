using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using Object = UnityEngine.Object;

// Сквозной тест в Editor Play Mode с виртуальными XR-контроллерами.
public static class ZelenPlayModeSmokeTest
{
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    private const string RunPendingKey = "ZelenPlayModeSmokeTest.RunPending";
    private static int failures;
    private static int oldSpamMessages;
    private static bool finishing;
    private static double startedAt;
    private static bool oldOptionsEnabled;
    private static EnterPlayModeOptions oldOptions;

    public static void Run()
    {
        Debug.Log("Zelen smoke: команда запуска получена.");
        SessionState.SetBool(RunPendingKey, true);
        EditorApplication.update -= BeginAfterReload;
        EditorApplication.update += BeginAfterReload;
    }

    [InitializeOnLoadMethod]
    private static void ResumeAfterReload()
    {
        if (SessionState.GetBool(RunPendingKey, false))
        {
            EditorApplication.update -= BeginAfterReload;
            EditorApplication.update += BeginAfterReload;
        }
    }

    private static void BeginAfterReload()
    {
        if (!SessionState.GetBool(RunPendingKey, false) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        EditorApplication.update -= BeginAfterReload;
        SessionState.SetBool(RunPendingKey, false);
        failures = 0;
        oldSpamMessages = 0;
        finishing = false;
        startedAt = EditorApplication.timeSinceStartup;
        oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
        oldOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        Application.logMessageReceived += OnLogMessage;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.update += Watchdog;
        EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
        // XR Device Simulator ожидает клавиатуру и мышь даже при управлении
        // виртуальными контроллерами; в batchmode этих устройств нет.
        if (Keyboard.current == null) InputSystem.AddDevice<Keyboard>();
        if (Mouse.current == null) InputSystem.AddDevice<Mouse>();
        Debug.Log("Zelen smoke: запуск Play Mode.");
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && !finishing)
        {
            GameObject host = new GameObject("ZelenSmokeTestRunner");
            Object.DontDestroyOnLoad(host);
            host.AddComponent<Runner>();
        }
        else if ((state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode) && finishing)
        {
            FinishEditor(failures == 0 ? 0 : 2);
        }
    }

    private static void Watchdog()
    {
        if (!finishing && EditorApplication.timeSinceStartup - startedAt > 180.0)
        {
            failures++;
            Debug.LogError("Zelen smoke FAIL: превышено время выполнения теста.");
            Finish();
        }
        else if (finishing && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            FinishEditor(failures == 0 ? 0 : 2);
        }
    }

    private static void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        if (condition.StartsWith("Action performed:", StringComparison.Ordinal))
        {
            oldSpamMessages++;
        }

        if (finishing || condition.StartsWith("Zelen smoke", StringComparison.Ordinal))
        {
            return;
        }

        if (type == LogType.Exception || type == LogType.Error)
        {
            // Отделяем известные ошибки вспомогательных редакторских пакетов
            // от ошибок игрового кода, не скрывая остальные исключения.
            bool editorPackageError = stackTrace.Contains("Unity.XR.CoreUtils.Editor.PackageVersionUtility") ||
                                      stackTrace.Contains("UnityEditor.Android.TargetExtension") ||
                                      stackTrace.Contains("UnityEditorInternal.SysrootManager");
            if (editorPackageError)
            {
                Debug.Log("Zelen smoke: проигнорирована ошибка редакторского пакета: " + condition);
                return;
            }
            failures++;
            Debug.Log("Zelen smoke RUNTIME ERROR: " + condition + "\n" + stackTrace);
        }
    }

    private static void Check(bool condition, string description)
    {
        if (condition)
        {
            Debug.Log("Zelen smoke OK: " + description);
        }
        else
        {
            failures++;
            Debug.LogError("Zelen smoke FAIL: " + description);
        }
    }

    private static void Finish()
    {
        if (finishing)
        {
            return;
        }
        finishing = true;
        Check(oldSpamMessages < 100, "Старый PrefabSpawner не переполняет журнал (" + oldSpamMessages + " сообщений)");
        Debug.Log("Zelen smoke: завершено, ошибок " + failures + ".");
        EditorApplication.isPlaying = false;
    }

    private static void FinishEditor(int exitCode)
    {
        Application.logMessageReceived -= OnLogMessage;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.update -= Watchdog;
        EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled;
        EditorSettings.enterPlayModeOptions = oldOptions;
        EditorApplication.Exit(exitCode);
    }

    private static IEnumerator Frames(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return null;
        }
    }

    private static XRSimulatedController Controller(string hand)
    {
        return InputSystem.devices.OfType<XRSimulatedController>()
            .FirstOrDefault(device => device.usages.Any(usage => usage.ToString() == hand));
    }

    private static void Press(ButtonControl button)
    {
        SetButton(button, true);
    }

    private static void Release(ButtonControl button)
    {
        SetButton(button, false);
    }

    private static void SetButton(ButtonControl button, bool pressed)
    {
        XRSimulatedController controller = button != null ? button.device as XRSimulatedController : null;
        if (controller == null || !Enum.TryParse(button.name, true, out ControllerButton controllerButton)) return;
        XRSimulatedControllerState state = new XRSimulatedControllerState
        {
            primary2DAxis = controller.primary2DAxis.ReadValue(),
            trigger = controller.trigger.ReadValue(),
            grip = controller.grip.ReadValue(),
            secondary2DAxis = controller.secondary2DAxis.ReadValue(),
            trackingState = controller.trackingState.ReadValue(),
            isTracked = controller.isTracked.isPressed,
            devicePosition = controller.devicePosition.ReadValue(),
            deviceRotation = controller.deviceRotation.ReadValue()
        };
        InputSystem.QueueStateEvent(controller, state.WithButton(controllerButton, pressed));
        InputSystem.Update();
    }

    private static int FirstLayer(LayerMask mask)
    {
        for (int i = 0; i < 32; i++)
        {
            if ((mask.value & (1 << i)) != 0)
            {
                return i;
            }
        }
        return 0;
    }

    private static void DebugUiRay(string stage, XRRayInteractor ray)
    {
        bool hasModel = ray.TryGetUIModel(out var model);
        string hit = hasModel && model.currentRaycast.gameObject != null ? model.currentRaycast.gameObject.name : "none";
        Debug.Log("Zelen smoke UI DIAG: " + stage + ", model=" + hasModel + ", hit=" + hit +
                  ", position=" + model.position + ", orientation=" + model.orientation +
                  ", points=" + model.raycastPoints.Count + ", target=" + model.currentRaycast.worldPosition +
                  ", press=" + ray.uiPressInput.ReadValue() + ", performed=" + ray.uiPressInput.ReadIsPerformed());
    }

    private sealed class Runner : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return Frames(45);
            Check(SceneManager.GetActiveScene().name == "MainMenu", "Запускается MainMenu");
            Check(Object.FindObjectsOfType<Transform>(true).Any(item => item.name == "XR Device Simulator"),
                "XR Device Simulator присутствует в MainMenu");

            GlavnoeMenu main = Object.FindObjectsOfType<GlavnoeMenu>().FirstOrDefault();
            Check(main != null, "Главное меню активно");
            if (main == null)
            {
                Finish();
                yield break;
            }
            Check(main.panel != null && main.panel.gameObject.activeInHierarchy, "Стартовая панель видима");
            Check(main.knopkaIgrat != null && main.knopkaIgrat.interactable, "Кнопка Играть доступна");

            XRSimulatedController left = Controller("LeftHand");
            XRSimulatedController right = Controller("RightHand");
            Check(left != null, "Создан левый виртуальный контроллер");
            Check(right != null, "Создан правый виртуальный контроллер");

            main.knopkaIgrat.onClick.Invoke();
            yield return Frames(60);
            Check(SceneManager.GetActiveScene().name == "BrightDay", "Кнопка Играть загружает BrightDay");

            // В пакетном режиме симулятор создаёт контроллеры без флага tracking.
            // Включаем его, как при подключении настоящих контроллеров PCVR.
            foreach (XRSimulatedController controller in InputSystem.devices.OfType<XRSimulatedController>())
            {
                InputSystem.QueueDeltaStateEvent(controller.isTracked, (byte)1);
                InputSystem.QueueDeltaStateEvent(controller.trackingState, 3);
            }
            InputSystem.Update();
            foreach (Transform controllerRoot in Object.FindObjectsOfType<Transform>(true)
                         .Where(item => item.name == "Left Controller" || item.name == "Right Controller"))
            {
                controllerRoot.gameObject.SetActive(true);
            }
            yield return Frames(5);

            IgrovoeMenu gameMenu = Object.FindObjectsOfType<IgrovoeMenu>().FirstOrDefault();
            PosadkaDereva planting = Object.FindObjectsOfType<PosadkaDereva>().FirstOrDefault(item => item.isActiveAndEnabled);
            VyborNasazhdeniya selector = Object.FindObjectsOfType<VyborNasazhdeniya>().FirstOrDefault(item => item.isActiveAndEnabled);
            foreach (MonoBehaviour tool in Object.FindObjectsOfType<MonoBehaviour>(true)
                         .Where(item => item is PosadkaDereva || item is VyborNasazhdeniya))
            {
                List<string> hierarchy = new List<string>();
                for (Transform current = tool.transform; current != null; current = current.parent)
                {
                    hierarchy.Insert(0, current.name + "[" + current.gameObject.activeSelf + "]");
                }
                Debug.Log("Zelen smoke DIAG: " + string.Join("/", hierarchy) + ", enabled=" + tool.enabled);
            }
            Debug.Log("Zelen smoke DIAG: simulator=" + (Object.FindObjectOfType<XRDeviceSimulator>() != null) +
                      ", controllers=" + string.Join(",", InputSystem.devices.OfType<XRSimulatedController>().Select(item => item.name + ":" + item.added + ":" + item.enabled)));
            Check(gameMenu != null, "Игровое меню существует");
            Check(planting != null, "Активна одна механика посадки");
            Check(selector != null, "Активна механика выбора насаждений");
            Check(Object.FindObjectsOfType<PosadkaDereva>().Count(item => item.isActiveAndEnabled) == 1,
                "Нет дублирования механики посадки");
            Check(Object.FindObjectsOfType<VyborNasazhdeniya>().Count(item => item.isActiveAndEnabled) == 1,
                "Нет дублирования механики выбора");
            if (gameMenu == null || planting == null || selector == null)
            {
                Finish();
                yield break;
            }

            Check(gameMenu.panelMenu != null && !gameMenu.panelMenu.activeSelf, "Игровое меню скрыто при запуске");
            Check(gameMenu.panelInformacii != null && !gameMenu.panelInformacii.activeSelf, "Информация скрыта при запуске");
            Check(!SostoyanieMenyu.GameMenuOpen, "Флаг игрового меню сброшен");
            NearFarInteractor[] standardNearFars = Object.FindObjectsOfType<NearFarInteractor>(true);
            bool[] initialNearFarStates = standardNearFars.Select(item => item.enabled).ToArray();

            gameMenu.knopkaInformaciya.onClick.Invoke();
            Check(gameMenu.panelInformacii.activeSelf, "Кнопка Информация открывает инструкцию");
            gameMenu.knopkaNazadOtInformacii.onClick.Invoke();
            Check(!gameMenu.panelInformacii.activeSelf, "Кнопка Назад закрывает инструкцию");

            gameMenu.Otkryt();
            Check(gameMenu.panelMenu.activeSelf && SostoyanieMenyu.GameMenuOpen, "Игровое меню открывается");
            gameMenu.knopkaPodskazki.onClick.Invoke();
            Check(gameMenu.panelKratkihPodskazok.activeSelf, "Краткие подсказки включаются");
            gameMenu.knopkaPodskazki.onClick.Invoke();
            Check(!gameMenu.panelKratkihPodskazok.activeSelf, "Краткие подсказки выключаются");
            gameMenu.knopkaProdolzhit.onClick.Invoke();
            Check(!gameMenu.panelMenu.activeSelf && !SostoyanieMenyu.GameMenuOpen, "Продолжить закрывает меню");

            left = Controller("LeftHand");
            right = Controller("RightHand");
            if (left != null)
            {
                Press(left.menuButton);
                Release(left.menuButton);
                yield return Frames(3);
                Check(gameMenu.panelMenu.activeSelf, "OpenGameMenu срабатывает от левой MenuButton");
                Press(left.menuButton);
                Release(left.menuButton);
                yield return Frames(3);
                Check(!gameMenu.panelMenu.activeSelf, "Повторная MenuButton закрывает меню");
            }

            right = Controller("RightHand");
            if (right != null)
            {
                Press(right.menuButton);
                Release(right.menuButton);
                yield return Frames(3);
                Check(gameMenu.panelMenu.activeSelf && SostoyanieMenyu.GameMenuOpen,
                    "OpenGameMenu срабатывает от правой MenuButton");

                Transform menuCamera = gameMenu.kamera;
                Vector3 oldCameraPosition = menuCamera.position;
                Quaternion oldCameraRotation = menuCamera.rotation;
                menuCamera.position += new Vector3(2f, 0.5f, -1f);
                menuCamera.rotation = Quaternion.Euler(12f, 47f, 0f);
                yield return Frames(3);
                Vector3 expectedPosition = menuCamera.position + menuCamera.forward * gameMenu.rasstoyanieDoMenyu;
                Quaternion expectedRotation = Quaternion.LookRotation(menuCamera.forward, menuCamera.up);
                Check(Vector3.Distance(gameMenu.panelMenu.transform.position, expectedPosition) < 0.001f &&
                      Quaternion.Angle(gameMenu.panelMenu.transform.rotation, expectedRotation) < 0.1f,
                    "Игровое меню следует за положением и поворотом камеры");

                InputAction placeAction = gameMenu.naborDeistvii.FindAction("XRI Left Interaction/PlaceTool");
                InputAction pickAction = gameMenu.naborDeistvii.FindAction("XRI Right Interaction/PickTool");
                InputAction moveAction = gameMenu.naborDeistvii.FindAction("XRI Left Locomotion/Move");
                InputAction leftUiPress = gameMenu.naborDeistvii.FindAction("XRI Left Interaction/UI Press");
                InputAction rightUiPress = gameMenu.naborDeistvii.FindAction("XRI Right Interaction/UI Press");
                InputAction leftSelect = gameMenu.naborDeistvii.FindAction("XRI Left Interaction/Select");
                InputAction rightSelect = gameMenu.naborDeistvii.FindAction("XRI Right Interaction/Select");
                Check(placeAction != null && !placeAction.enabled &&
                      pickAction != null && !pickAction.enabled &&
                      moveAction != null && !moveAction.enabled,
                    "В игровом меню заблокированы посадка выбор и передвижение");
                Check(leftUiPress != null && leftUiPress.enabled && rightUiPress != null && rightUiPress.enabled &&
                      leftSelect != null && leftSelect.enabled && rightSelect != null && rightSelect.enabled,
                    "UI-действия и триггеры обоих контроллеров остаются активны");

                XRRayInteractor[] menuRays = Object.FindObjectsOfType<XRRayInteractor>(true)
                    .Where(item => item.name.StartsWith("Menu Ray - ", StringComparison.Ordinal)).ToArray();
                Debug.Log("Zelen smoke DIAG: menu rays=" + string.Join(", ", menuRays.Select(item =>
                    item.name + "[active=" + item.gameObject.activeInHierarchy + ", enabled=" + item.enabled + "]")));
                Check(menuRays.Length == 2 && menuRays.All(item => item.isActiveAndEnabled && item.enableUIInteraction),
                    "При открытии меню активны два независимых UI-луча");
                Check(menuRays.Length == 2 && menuRays.All(item => item.GetComponent<XRInteractorLineVisual>() != null &&
                                           item.GetComponent<XRInteractorLineVisual>().isActiveAndEnabled &&
                                           item.GetComponent<LineRenderer>() != null),
                    "Оба меню-луча имеют активную видимую линию");
                Check(Object.FindObjectsOfType<NearFarInteractor>().All(item => !item.enabled),
                    "Обычные XR-интеракторы отключены на время меню");

                XRRayInteractor rightMenuRay = menuRays.FirstOrDefault(item => item.name.Contains("Right"));
                XRRayInteractor leftMenuRay = menuRays.FirstOrDefault(item => item.name.Contains("Left"));
                if (rightMenuRay != null)
                {
                    rightMenuRay.transform.rotation = Quaternion.LookRotation(
                        gameMenu.knopkaInformaciya.transform.position - rightMenuRay.transform.position);
                    yield return Frames(6);
                    DebugUiRay("right before, button=" + gameMenu.knopkaInformaciya.transform.position +
                               ", panel=" + gameMenu.panelMenu.transform.position, rightMenuRay);
                    Press(right.triggerButton);
                    yield return Frames(2);
                    DebugUiRay("right pressed", rightMenuRay);
                    Release(right.triggerButton);
                    yield return Frames(3);
                    Check(gameMenu.panelInformacii.activeSelf, "Правый меню-луч нажимает кнопку Информация");
                    gameMenu.SkrytInformaciyu();
                }
                if (leftMenuRay != null)
                {
                    leftMenuRay.transform.rotation = Quaternion.LookRotation(
                        gameMenu.knopkaInformaciya.transform.position - leftMenuRay.transform.position);
                    yield return Frames(6);
                    Press(left.triggerButton);
                    yield return Frames(2);
                    Release(left.triggerButton);
                    yield return Frames(3);
                    Check(gameMenu.panelInformacii.activeSelf, "Левый меню-луч нажимает кнопку Информация");
                    gameMenu.SkrytInformaciyu();
                }

                Press(right.secondaryButton);
                Release(right.secondaryButton);
                yield return Frames(3);
                Check(!SostoyanieMenyu.InventoryOpen && !SostoyanieMenyu.ToolsMenuOpen,
                    "Из игрового меню нельзя открыть другие игровые панели");

                Press(right.menuButton);
                Release(right.menuButton);
                yield return Frames(3);
                Check(!gameMenu.panelMenu.activeSelf && !SostoyanieMenyu.GameMenuOpen,
                    "Повторная правая MenuButton закрывает меню");
                Check(placeAction.enabled && pickAction.enabled && moveAction.enabled,
                    "После закрытия меню игровой ввод восстанавливается");
                Check(menuRays.All(item => item == null || !item.gameObject.activeSelf),
                    "После закрытия меню специальные лучи скрыты");
                Check(standardNearFars.Select((item, index) => item == null || item.enabled == initialNearFarStates[index]).All(item => item),
                    "После закрытия меню исходные состояния обычных XR-интеракторов восстановлены");
                menuCamera.position = oldCameraPosition;
                menuCamera.rotation = oldCameraRotation;
            }

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "ZelenSmokeGround";
            ground.transform.position = new Vector3(1000f, -0.5f, 1000f);
            ground.transform.localScale = new Vector3(30f, 1f, 30f);
            ground.layer = FirstLayer(planting.maskaTerreina);
            Check(planting.maskaTerreina.value != 0, "Маска террейна посадки настроена");

            GameObject template = GameObject.CreatePrimitive(PrimitiveType.Cube);
            template.name = "ZelenSmokeTreeTemplate";
            template.transform.position = new Vector3(1000f, -100f, 1000f);
            template.transform.localScale = new Vector3(0.5f, 2f, 0.5f);
            Nasazhdenie templateData = template.AddComponent<Nasazhdenie>();
            templateData.vidNasazhdeniya = "ZelenSmokeTree";
            templateData.kategoriya = "Деревья";

            GameObject leftRay = new GameObject("ZelenSmokeLeftRay");
            leftRay.transform.position = new Vector3(1000f, 5f, 1000f);
            leftRay.transform.rotation = Quaternion.LookRotation(Vector3.down);
            planting.tochkaStarta = leftRay.transform;
            planting.dlinaLucha = 20f;
            VyborInstrumenta.TekushiyPrefab = template;
            yield return Frames(5);

            left = Controller("LeftHand");
            if (left != null)
            {
                Press(left.triggerButton);
                Release(left.triggerButton);
            }
            yield return Frames(5);
            Nasazhdenie planted = Object.FindObjectsOfType<Nasazhdenie>()
                .FirstOrDefault(item => item.sourcePrefab == template && item.gameObject != template);
            Check(planted != null, "PlaceTool создаёт объект через левый триггер");
            if (planted != null)
            {
                Renderer renderer = planted.GetComponentInChildren<Renderer>();
                Check(renderer != null && Mathf.Abs(renderer.bounds.min.y) < 0.001f,
                    "Посаженный объект касается террейна нижней границей");
                Check(Mathf.DeltaAngle(planted.transform.eulerAngles.x, 0f) < 0.001f &&
                      Mathf.DeltaAngle(planted.transform.eulerAngles.z, 0f) < 0.001f,
                    "Посаженный объект строго вертикален");
            }

            PanelPereklyuchatel inventory = Object.FindObjectsOfType<PanelPereklyuchatel>()
                .FirstOrDefault(item => item.imyaDeistviya == "OpenInventory");
            Check(inventory != null, "Найден переключатель инвентаря");
            right = Controller("RightHand");
            if (right != null)
            {
                Press(right.secondaryButton);
                Release(right.secondaryButton);
                yield return Frames(5);
            }
            Check(inventory != null && inventory.panel.activeSelf && SostoyanieMenyu.InventoryOpen,
                "OpenInventory открывает инвентарь и активирует правый луч");

            if (planted != null)
            {
                GameObject rightRay = new GameObject("ZelenSmokeRightRay");
                Vector3 target = planted.transform.position;
                rightRay.transform.position = target + Vector3.up * 5f;
                rightRay.transform.rotation = Quaternion.LookRotation(Vector3.down);
                selector.tochkaStarta = rightRay.transform;
                selector.dlinaLucha = 20f;
                yield return Frames(5);

                PodsvetkaNasazhdeniya highlight = planted.GetComponent<PodsvetkaNasazhdeniya>();
                Check(highlight != null, "Наведение добавляет подсветку");
                if (highlight != null && selector.materialPodsvetki != null)
                {
                    Renderer highlightedRenderer = planted.GetComponentInChildren<Renderer>();
                    Check(highlightedRenderer.sharedMaterials.All(material => material == selector.materialPodsvetki),
                        "Наведение заменяет материалы на красный материал");
                }

                right = Controller("RightHand");
                if (right != null)
                {
                    Press(right.triggerButton);
                    Release(right.triggerButton);
                    yield return Frames(3);
                }
                Check(selector.inventar != null && selector.inventar.tekushiyObyekt == planted,
                    "PickTool выбирает объект под правым лучом");

                Quaternion originalRotation = planted.transform.rotation;
                Vector3 originalScale = planted.transform.localScale;
                right = Controller("RightHand");
                if (right != null)
                {
                    // Сначала зажимаем объект на прежней точке, затем двигаем луч.
                    Press(right.gripButton);
                    yield return Frames(2);
                    rightRay.transform.position = new Vector3(1003f, 5f, 1000f);
                    yield return Frames(6);
                    Release(right.gripButton);
                    yield return Frames(3);
                }
                Renderer movedRenderer = planted.GetComponentInChildren<Renderer>();
                Check(Vector3.Distance(new Vector3(planted.transform.position.x, 0f, planted.transform.position.z),
                        new Vector3(1003f, 0f, 1000f)) < 0.01f,
                    "MoveTool перемещает объект только по X и Z");
                Check(movedRenderer != null && Mathf.Abs(movedRenderer.bounds.min.y) < 0.001f,
                    "Перемещённый объект остаётся на террейне");
                Check(planted.transform.rotation == originalRotation && planted.transform.localScale == originalScale,
                    "Перемещение сохраняет rotation и scale");
            }

            gameMenu.Otkryt();
            yield return Frames(3);
            LineRenderer selectionLine = selector.GetComponent<LineRenderer>();
            Check(selectionLine != null && !selectionLine.enabled, "Игровое меню отключает луч выбора");
            gameMenu.knopkaGlavnoeMenu.onClick.Invoke();
            yield return Frames(45);
            Check(SceneManager.GetActiveScene().name == "MainMenu", "В главное меню возвращает в MainMenu");
            Check(!SostoyanieMenyu.GameMenuOpen && !SostoyanieMenyu.AnyMenuOpen,
                "После возврата состояние меню сброшено");

            VyborInstrumenta.TekushiyPrefab = null;
            Finish();
        }
    }
}
