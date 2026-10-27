using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

// Однократно добавляет симулятор в рабочие сцены из открытого редактора.
[InitializeOnLoad]
public static class ZelenDeviceSimulatorSetup
{
    private const string AutoKey = "Zelen.DeviceSimulatorAdded.v1";
    private const string SimulatorPath = "Assets/Samples/XR Interaction Toolkit/3.1.2/XR Device Simulator/XRDeviceSimulator/XR Device Simulator.prefab";
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Others/Supercyan Free Forest Sample/Scenes/BrightDay.unity"
    };

    static ZelenDeviceSimulatorSetup()
    {
        if (!EditorPrefs.GetBool(AutoKey, false))
        {
            EditorApplication.delayCall += AddAutomatically;
        }
    }

    private static void AddAutomatically()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorPrefs.GetBool(AutoKey, false))
        {
            return;
        }

        try
        {
            AddToAllScenes();
            EditorPrefs.SetBool(AutoKey, true);
        }
        catch (Exception exception)
        {
            Debug.LogError("Zelen simulator: не удалось добавить симулятор. " + exception);
        }
    }

    [MenuItem("Zelen/Добавить XR Device Simulator во все сцены")]
    public static void AddToAllScenes()
    {
        GameObject simulatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SimulatorPath);
        if (simulatorPrefab == null)
        {
            throw new InvalidOperationException("Не найден префаб: " + SimulatorPath);
        }

        // Сохраняем текущие визуальные правки пользователя перед переключением сцен.
        EditorSceneManager.SaveOpenScenes();
        SceneSetup[] originalSetup = EditorSceneManager.GetSceneManagerSetup();

        try
        {
            foreach (string scenePath in ScenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                bool alreadyAdded = Object.FindObjectsOfType<Transform>(true)
                    .Any(item => item.name == "XR Device Simulator");
                if (alreadyAdded)
                {
                    Debug.Log("Zelen simulator: уже есть в " + scenePath);
                    continue;
                }

                GameObject simulator = PrefabUtility.InstantiatePrefab(simulatorPrefab) as GameObject;
                if (simulator == null)
                {
                    throw new InvalidOperationException("Не удалось создать симулятор в " + scenePath);
                }
                simulator.name = "XR Device Simulator";
                simulator.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, scenePath);
                Debug.Log("Zelen simulator: добавлен в " + scenePath);
            }
        }
        finally
        {
            if (originalSetup.Length > 0)
            {
                EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
            }
        }

        Debug.Log("Zelen simulator: SUCCESS. Симулятор добавлен в MainMenu и BrightDay.");
    }
}
