using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// JSON persistence for GridCanvas contents. Files are written below
    /// Application.persistentDataPath and are intentionally versioned so future
    /// migrations can detect older documents.
    /// </summary>
    public static class GridSerializer
    {
        public const string DefaultFileName = "grid_snapshot.json";
        public const int CurrentVersion = 1;

        public static string GetDefaultPath()
        {
            return Path.Combine(Application.persistentDataPath, DefaultFileName);
        }

        public static bool Save(GridCanvas canvas, string fileName = DefaultFileName)
        {
            if (canvas == null)
            {
                Debug.LogError("GridSerializer.Save received a null canvas.");
                return false;
            }

            GridSnapshot snapshot = GridSnapshot.Create(canvas);
            return SaveSnapshot(snapshot, fileName);
        }

        public static bool SaveSnapshot(GridSnapshot snapshot, string fileName = DefaultFileName)
        {
            if (snapshot == null)
            {
                return false;
            }

            try
            {
                string path = ResolvePath(fileName);
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(snapshot, true);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError("GridSerializer failed to save snapshot: " + exception.Message);
                return false;
            }
        }

        public static GridSnapshot Load(string fileName = DefaultFileName)
        {
            try
            {
                string path = ResolvePath(fileName);
                if (!File.Exists(path))
                {
                    return null;
                }

                string json = File.ReadAllText(path);
                GridSnapshot snapshot = JsonUtility.FromJson<GridSnapshot>(json);
                if (snapshot != null && snapshot.version > CurrentVersion)
                {
                    Debug.LogWarning("Grid snapshot version " + snapshot.version + " is newer than supported " + CurrentVersion);
                }
                return snapshot;
            }
            catch (Exception exception)
            {
                Debug.LogError("GridSerializer failed to load snapshot: " + exception.Message);
                return null;
            }
        }

        /// <summary>Applies a loaded snapshot to a canvas, replacing its layers.</summary>
        public static bool Apply(GridSnapshot snapshot, GridCanvas canvas)
        {
            if (snapshot == null || canvas == null)
            {
                return false;
            }

            canvas.ClearAll();

            if (snapshot.layers == null)
            {
                return true;
            }

            for (int i = 0; i < snapshot.layers.Count; i++)
            {
                GridLayerSnapshot layerSnapshot = snapshot.layers[i];
                if (layerSnapshot == null)
                {
                    continue;
                }

                GridLayer layer = canvas.GetLayer(layerSnapshot.index);
                if (layer == null)
                {
                    layer = canvas.AddLayer(layerSnapshot.name);
                }

                layer.Name = layerSnapshot.name;
                layer.Visible = layerSnapshot.visible;
                layer.Locked = layerSnapshot.locked;
                layer.Tint = layerSnapshot.tint;
                layer.Palette = layerSnapshot.palette;

                List<GridCell> cells = new List<GridCell>();
                for (int c = 0; c < layerSnapshot.cells.Count; c++)
                {
                    cells.Add(layerSnapshot.cells[c].ToCell());
                }
                layer.LoadFrom(cells);
            }
            return true;
        }

        public static bool LoadInto(GridCanvas canvas, string fileName = DefaultFileName)
        {
            GridSnapshot snapshot = Load(fileName);
            if (snapshot == null)
            {
                return false;
            }
            return Apply(snapshot, canvas);
        }

        public static bool Delete(string fileName = DefaultFileName)
        {
            try
            {
                string path = ResolvePath(fileName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError("GridSerializer failed to delete snapshot: " + exception.Message);
                return false;
            }
        }

        private static string ResolvePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = DefaultFileName;
            }
            if (Path.IsPathRooted(fileName))
            {
                return fileName;
            }
            return Path.Combine(Application.persistentDataPath, fileName);
        }
    }
}
