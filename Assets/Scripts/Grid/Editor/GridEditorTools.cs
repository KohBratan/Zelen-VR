#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZelenVR.Grid.Editor
{
    /// <summary>
    /// Menu items that make the new grid modules easier to try in a scene.
    /// Everything here is editor-only and guarded by UNITY_EDITOR so player
    /// builds never reference UnityEditor APIs.
    /// </summary>
    public static class GridEditorTools
    {
        private const string GridFolder = "Assets/Scripts/Grid";

        [MenuItem("Tools/ZelenVR/Grid/Create Grid Settings Asset")]
        public static void CreateGridSettingsAsset()
        {
            GridSettings settings = CreateAsset<GridSettings>("GridSettings");
            if (settings != null)
            {
                Debug.Log("Created GridSettings at " + AssetDatabase.GetAssetPath(settings), settings);
            }
        }

        [MenuItem("Tools/ZelenVR/Grid/Create Snap Profile Asset")]
        public static void CreateSnapProfileAsset()
        {
            SnapProfile profile = CreateAsset<SnapProfile>("SnapProfile");
            if (profile != null)
            {
                Debug.Log("Created SnapProfile at " + AssetDatabase.GetAssetPath(profile), profile);
            }
        }

        [MenuItem("Tools/ZelenVR/Grid/Create Color Palette Asset")]
        public static void CreateColorPaletteAsset()
        {
            GridColorPalette palette = CreateAsset<GridColorPalette>("GridColorPalette");
            if (palette != null)
            {
                Debug.Log("Created GridColorPalette at " + AssetDatabase.GetAssetPath(palette), palette);
            }
        }

        [MenuItem("Tools/ZelenVR/Grid/Validate Selected Settings")]
        public static void ValidateSelectedSettings()
        {
            GridSettings settings = Selection.activeObject as GridSettings;
            if (settings == null)
            {
                Debug.LogWarning("Select a GridSettings asset in the Project window first.");
                return;
            }

            GridValidationReport report = GridValidation.ValidateSettings(settings);
            LogReport("Grid settings validation", report, settings);
        }

        [MenuItem("Tools/ZelenVR/Grid/Validate Scene Canvases")]
        public static void ValidateSceneCanvases()
        {
            GridCanvas[] canvases = Object.FindObjectsOfType<GridCanvas>();
            if (canvases == null || canvases.Length == 0)
            {
                Debug.LogWarning("No GridCanvas found in the open scene.");
                return;
            }

            for (int i = 0; i < canvases.Length; i++)
            {
                GridValidationReport report = GridValidation.ValidateCanvas(canvases[i]);
                LogReport("Grid canvas validation", report, canvases[i]);
            }
        }

        [MenuItem("Tools/ZelenVR/Grid/Log Scene Grid Stats")]
        public static void LogSceneGridStats()
        {
            GridCanvas canvas = Object.FindObjectOfType<GridCanvas>();
            if (canvas == null)
            {
                Debug.LogWarning("No GridCanvas found in the open scene.");
                return;
            }

            string summary = canvas.Stats != null ? canvas.Stats.ToSummary() : "no stats";
            Debug.Log("GridCanvas layers=" + canvas.LayerCount + " " + summary, canvas);
        }

        private static T CreateAsset<T>(string defaultName) where T : ScriptableObject
        {
            EnsureFolder();
            string path = AssetDatabase.GenerateUniqueAssetPath(GridFolder + "/" + defaultName + ".asset");
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        private static void EnsureFolder()
        {
            if (AssetDatabase.IsValidFolder(GridFolder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(GridFolder);
            string name = Path.GetFileName(GridFolder);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
            {
                AssetDatabase.CreateFolder(parent.Replace('\\', '/'), name);
            }
        }

        private static void LogReport(string title, GridValidationReport report, Object context)
        {
            if (report == null)
            {
                return;
            }

            string message = title + ": " + report.ToSummary() + "\n" + report;
            if (report.HasErrors)
            {
                Debug.LogError(message, context);
            }
            else if (report.WarningCount > 0)
            {
                Debug.LogWarning(message, context);
            }
            else
            {
                Debug.Log(message, context);
            }
        }
    }
}
#endif
