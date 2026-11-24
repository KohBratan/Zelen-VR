using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>Serializable mirror of GridCell used by the save system.</summary>
    [Serializable]
    public struct GridCellSnapshot
    {
        public int x;
        public int y;
        public int state;
        public int layer;
        public int palette;
        public float fill;

        public GridCellSnapshot(GridCell cell)
        {
            x = cell.Coordinate.X;
            y = cell.Coordinate.Y;
            state = (int)cell.State;
            layer = cell.Layer;
            palette = cell.Palette;
            fill = cell.Fill;
        }

        public GridCell ToCell()
        {
            return new GridCell(new GridCoordinate(x, y), (GridCellState)state, layer, palette, fill);
        }
    }

    /// <summary>Serializable mirror of one GridLayer.</summary>
    [Serializable]
    public class GridLayerSnapshot
    {
        public string name;
        public int index;
        public bool visible;
        public bool locked;
        public Color tint;
        public int palette;
        public List<GridCellSnapshot> cells = new List<GridCellSnapshot>();

        public static GridLayerSnapshot FromLayer(GridLayer layer)
        {
            GridLayerSnapshot snapshot = new GridLayerSnapshot();
            if (layer == null)
            {
                return snapshot;
            }

            snapshot.name = layer.Name;
            snapshot.index = layer.Index;
            snapshot.visible = layer.Visible;
            snapshot.locked = layer.Locked;
            snapshot.tint = layer.Tint;
            snapshot.palette = layer.Palette;

            foreach (KeyValuePair<GridCoordinate, GridCell> pair in layer.EnumerateCells())
            {
                snapshot.cells.Add(new GridCellSnapshot(pair.Value));
            }
            return snapshot;
        }
    }

    /// <summary>Root object written to disk by GridSerializer.</summary>
    [Serializable]
    public class GridSnapshot
    {
        public int version = 1;
        public string savedAtUtc;
        public int width;
        public int height;
        public float cellSize;
        public int plane;
        public List<GridLayerSnapshot> layers = new List<GridLayerSnapshot>();

        public static GridSnapshot Create(GridCanvas canvas)
        {
            GridSnapshot snapshot = new GridSnapshot();
            snapshot.savedAtUtc = DateTime.UtcNow.ToString("o");

            if (canvas == null || canvas.Settings == null)
            {
                return snapshot;
            }

            snapshot.width = canvas.Settings.Width;
            snapshot.height = canvas.Settings.Height;
            snapshot.cellSize = canvas.Settings.CellSize;
            snapshot.plane = (int)canvas.Settings.Plane;

            for (int i = 0; i < canvas.LayerCount; i++)
            {
                snapshot.layers.Add(GridLayerSnapshot.FromLayer(canvas.GetLayer(i)));
            }
            return snapshot;
        }

        public int TotalCellCount()
        {
            int total = 0;
            for (int i = 0; i < layers.Count; i++)
            {
                total += layers[i].cells.Count;
            }
            return total;
        }

        public bool IsEmpty
        {
            get { return layers == null || layers.Count == 0 || TotalCellCount() == 0; }
        }
    }
}
