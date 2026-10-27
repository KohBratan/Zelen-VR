using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Сборка PCVR Windows x64. Использует включённые сцены из Build Settings.
public static class ZelenBuild
{
    [MenuItem("Zelen/Собрать PCVR Windows x64 — VR Build 2.1")]
    public static void SobratPCVR()
    {
        string outputDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Zelen VR Build 2.1");
        string executablePath = Path.Combine(outputDirectory, "ZelenVR.exe");
        Directory.CreateDirectory(outputDirectory);

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("В Build Settings нет включённых сцен.");
        }

        Debug.Log("Zelen build: старт Windows x64. Сцены: " + string.Join(", ", scenes));
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = executablePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log("Zelen build: результат " + summary.result + ", ошибок " + summary.totalErrors +
                  ", размер " + summary.totalSize + " байт, время " + summary.totalTime + ".");

        if (summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("Zelen build: сборка не удалась. Подробности в Editor.log.");
            EditorApplication.Exit(2);
            return;
        }

        Debug.Log("Zelen build: SUCCESS. Файл: " + executablePath);
    }
}
