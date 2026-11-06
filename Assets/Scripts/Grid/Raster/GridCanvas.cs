using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Aggregates grid layers and exposes high level paint/erase operations
    /// that accept a DrawStroke. The canvas owns no rendering and no input; it
    /// is the bridge between stroke producers and the cell storage used for
    /// gameplay queries and persistence.
    /// </summary>
    [DisallowMultipleComponent]
    public class GridCanvas : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager manager;
        [SerializeField] private GridSettings settings;

        [Header("Brush")]
        [SerializeField] private GridBrush brush = GridBrush.Single;
        [SerializeField] private int activePalette = 0;

        [Header("Rasterization")]
        [SerializeField] private bool useSupercover = true;

        private readonly List<GridLayer> layers = new List<GridLayer>();
        private readonly GridRasterStats stats = new GridRasterStats();

        public GridManager Manager { get { return manager; } }
        public GridSettings Settings { get { return settings; } }
        public GridBrush Brush { get { return brush; } set { brush = value; } }
        public int ActivePalette { get { return activePalette; } set { activePalette = value; } }
        public int LayerCount { get { return layers.Count; } }
        public GridRasterStats Stats { get { return stats; } }

        public event Action<GridLayer, IReadOnlyList<GridCoordinate>> CellsChanged;
        public event Action<int> LayerAdded;

        private void Awake()
        {
            if (manager == null)
            {
                manager = GetComponent<GridManager>();
            }
            if (manager != null && settings == null)
            {
                settings = manager.Settings;
            }
            if (layers.Count == 0)
            {
                AddLayer("Default");
            }
        }

        public GridLayer AddLayer(string layerName)
        {
            GridLayer layer = new GridLayer(layerName, layers.Count);
            layers.Add(layer);
            if (LayerAdded != null)
            {
                LayerAdded(layer.Index);
            }
            return layer;
        }

        public GridLayer GetLayer(int index)
        {
            if (index < 0 || index >= layers.Count)
            {
                return null;
            }
            return layers[index];
        }

        public GridLayer GetLayer(string layerName)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].Name == layerName)
                {
                    return layers[i];
                }
            }
            return null;
        }

        public void ClearAll()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].Clear();
            }
        }

        /// <summary>
        /// Rasterizes a stroke into the target layer, then mirrors every painted
        /// cell into the GridManager so other systems observe the change.
        /// </summary>
        public int PaintStroke(DrawStroke stroke, int layerIndex = 0)
        {
            if (stroke == null || settings == null)
            {
                return 0;
            }

            float started = Time.realtimeSinceStartup;
            GridLayer layer = GetLayer(layerIndex);
            if (layer == null)
            {
                layer = AddLayer("Layer" + layerIndex);
            }

            List<GridCoordinate> centres = GridRasterizer.RasterizeStroke(stroke, settings, useSupercover);
            HashSet<GridCoordinate> affected = GridRasterizer.ApplyBrush(centres, brush, settings);

            List<GridCoordinate> changed = new List<GridCoordinate>();
            int painted = 0;
            int skipped = 0;
            int duplicates = 0;

            foreach (GridCoordinate coordinate in affected)
            {
                if (layer.IsOccupied(coordinate))
                {
                    duplicates++;
                    continue;
                }

                if (!layer.Paint(coordinate, activePalette))
                {
                    skipped++;
                    continue;
                }

                painted++;
                changed.Add(coordinate);
                if (manager != null)
                {
                    manager.SetCell(GridCell.CreatePainted(coordinate, layer.Index, activePalette), true);
                }
            }

            float duration = Time.realtimeSinceStartup - started;
            stats.RecordStroke(stroke, centres.Count, painted, skipped, duplicates, duration);

            if (changed.Count > 0 && CellsChanged != null)
            {
                CellsChanged(layer, changed);
            }
            return painted;
        }

        /// <summary>Erases all cells covered by the stroke on the given layer.</summary>
        public int EraseStroke(DrawStroke stroke, int layerIndex = 0)
        {
            if (stroke == null || settings == null)
            {
                return 0;
            }

            GridLayer layer = GetLayer(layerIndex);
            if (layer == null)
            {
                return 0;
            }

            List<GridCoordinate> centres = GridRasterizer.RasterizeStroke(stroke, settings, useSupercover);
            HashSet<GridCoordinate> affected = GridRasterizer.ApplyBrush(centres, brush, settings);

            List<GridCoordinate> changed = new List<GridCoordinate>();
            foreach (GridCoordinate coordinate in affected)
            {
                if (layer.Erase(coordinate))
                {
                    changed.Add(coordinate);
                    if (manager != null)
                    {
                        manager.RemoveCell(coordinate, true);
                    }
                }
            }

            if (changed.Count > 0 && CellsChanged != null)
            {
                CellsChanged(layer, changed);
            }
            return changed.Count;
        }

        public bool IsOccupied(GridCoordinate coordinate)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].IsOccupied(coordinate))
                {
                    return true;
                }
            }
            return false;
        }

        public GridBounds CalculateBounds()
        {
            GridBounds result = GridBounds.Empty;
            for (int i = 0; i < layers.Count; i++)
            {
                result.Encapsulate(layers[i].CalculateBounds());
            }
            return result;
        }
    }
}
