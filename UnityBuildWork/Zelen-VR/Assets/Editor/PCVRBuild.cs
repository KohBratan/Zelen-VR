using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;

public static class PCVRBuild
{
    public static void Build()
    {
        try
        {
            const string outputPath = @"C:\Users\layba\Documents\Zelen_VR_build_0.3\Zelen_VR_build_0.3.exe";

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes in Build Settings.");

            foreach (var scenePath in scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.name.StartsWith("XR Device Simulator", StringComparison.OrdinalIgnoreCase))
                    {
                        root.SetActive(false);
                        Debug.Log($"PCVR_BUILD: Disabled '{root.name}' in {scenePath}");
                    }
                }
                EditorSceneManager.SaveScene(scene);
            }

            var simulatorSettings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                "Assets/Others/XRI/Settings/Resources/XRDeviceSimulatorSettings.asset");
            var serializedSimulatorSettings = simulatorSettings == null ? null : new SerializedObject(simulatorSettings);
            var automaticInstantiation = serializedSimulatorSettings?.FindProperty("m_AutomaticallyInstantiateSimulatorPrefab");
            if (automaticInstantiation == null || automaticInstantiation.boolValue)
                throw new InvalidOperationException("XR Device Simulator automatic instantiation is still enabled.");

            var xrSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (xrSettings == null || !xrSettings.InitManagerOnStart)
                throw new InvalidOperationException("XR Plug-in Management is not initialized on startup for Standalone.");

            var loaders = xrSettings.Manager.activeLoaders.Select(loader => loader.name).ToArray();
            if (!loaders.Any(loader => loader.Replace(" ", "") == "OpenXRLoader"))
                throw new InvalidOperationException("OpenXR Loader is not enabled for Standalone.");

            Debug.Log($"PCVR_BUILD: Scenes: {string.Join(", ", scenes)}");
            Debug.Log($"PCVR_BUILD: XR loaders: {string.Join(", ", loaders)}");
            Debug.Log("PCVR_BUILD: XR Device Simulator automatic instantiation: OFF");
            Debug.Log("PCVR_BUILD: Configuration: Release, Windows x64, OpenXR");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log($"PCVR_BUILD: Result={summary.result}; Size={summary.totalSize}; Errors={summary.totalErrors}; Warnings={summary.totalWarnings}; Output={summary.outputPath}");

            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
        catch (Exception exception)
        {
            Debug.LogError($"PCVR_BUILD: {exception}");
            EditorApplication.Exit(2);
        }
    }
}
