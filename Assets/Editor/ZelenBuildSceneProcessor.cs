using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using Object = UnityEngine.Object;

// Удаляет устройства симуляции только из временной копии сцен при сборке.
// Сцены проекта и тестирование в редакторе остаются без изменений.
public sealed class ZelenBuildSceneProcessor : IProcessSceneWithReport
{
    public int callbackOrder => 0;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (report == null)
        {
            return;
        }

        int removed = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (XRDeviceSimulator simulator in root.GetComponentsInChildren<XRDeviceSimulator>(true))
            {
                if (simulator != null)
                {
                    Object.DestroyImmediate(simulator.gameObject);
                    removed++;
                }
            }

            // XR Device Simulator и LifecycleManager обычно находятся на одном корневом объекте.
            if (root == null)
            {
                continue;
            }

            foreach (SimulatedDeviceLifecycleManager manager in root.GetComponentsInChildren<SimulatedDeviceLifecycleManager>(true))
            {
                if (manager != null)
                {
                    Object.DestroyImmediate(manager.gameObject);
                    removed++;
                }
            }
        }

        Debug.Log("Zelen build: симуляторы удалены из сцены " + scene.name + ", объектов: " + removed + ".");
    }
}
