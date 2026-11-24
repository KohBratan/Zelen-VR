// ---------------------------------------------------------------------------
// Zelen-VR
// File: GridLayer.cs
// Sloy setki.
// ---------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// One named layer of painted cells. Layers are independent so features
    /// such as terrain, decoration or collision hints can be toggled without
    /// discarding each other's data.
    /// </summary>
    [Serializable]
    public class GridLayer
    {
        [SerializeField] private string name = "Layer";
        [SerializeField] private int index;
        [SerializeField] private bool visible = true;
        [SerializeField] private bool locked = false;
        [SerializeField] private Color tint = Color.white;
        [SerializeField] private int palette;

        private readonly Dictionary<GridCoordinate, GridCell> cells = new Dictionary<GridCoordinate, GridCell>();

        public string Name { get { return name; } set { name = value; } }
        public int Index { get { return index; } set { index = value; } }
        public bool Visible { get { return visible; } set { visible = value; } }
        public bool Locked { get { return locked; } set { locked = value; } }
        public Color Tint { get { return tint; } set { tint = value; } }
        public int Palette { get { return palette; } set { palette = value; } }
        public int CellCount { get { return cells.Count; } }

        public GridLayer()
        {
        }

        public GridLayer(string name, int index)
        {
            this.name = name;
            this.index = index;
        }

        public bool IsOccupied(GridCoordinate coordinate)
        {
            GridCell cell;
            return cells.TryGetValue(coordinate, out cell) && cell.IsOccupied;
        }

        public bool Contains(GridCoordinate coordinate)
        {
            return cells.ContainsKey(coordinate);
        }

        public GridCell Get(GridCoordinate coordinate)
        {
            GridCell cell;
            if (cells.TryGetValue(coordinate, out cell))
            {
                return cell;
            }
            return GridCell.CreateEmpty(coordinate);
        }

        public bool Paint(GridCoordinate coordinate)
        {
            if (locked || !visible)
            {
                return false;
            }

            GridCell cell = GridCell.CreatePainted(coordinate, index, palette);
            if (cells.TryGetValue(coordinate, out GridCell previous) && previous.Equals(cell))
            {
                return false;
            }
            cells[coordinate] = cell;
            return true;
        }

        public bool Paint(GridCoordinate coordinate, int newPalette)
        {
            if (locked || !visible)
            {
                return false;
            }

            cells[coordinate] = GridCell.CreatePainted(coordinate, index, newPalette);
            return true;
        }

        public bool Erase(GridCoordinate coordinate)
        {
            if (locked)
            {
                return false;
            }
            return cells.Remove(coordinate);
        }

        public void Clear()
        {
            cells.Clear();
        }

        public IEnumerable<KeyValuePair<GridCoordinate, GridCell>> EnumerateCells()
        {
            return cells;
        }

        public GridBounds CalculateBounds()
        {
            if (cells.Count == 0)
            {
                return GridBounds.Empty;
            }

            GridBounds bounds = GridBounds.Empty;
            foreach (KeyValuePair<GridCoordinate, GridCell> pair in cells)
            {
                bounds.Encapsulate(pair.Key);
            }
            return bounds;
        }

        /// <summary>Replaces the layer contents with the provided dictionary data.</summary>
        public void LoadFrom(IEnumerable<GridCell> loadedCells)
        {
            cells.Clear();
            if (loadedCells == null)
            {
                return;
            }
            foreach (GridCell cell in loadedCells)
            {
                if (cell.IsOccupied)
                {
                    cells[cell.Coordinate] = cell;
                }
            }
        }

        public override string ToString()
        {
            return "GridLayer(" + name + ", index=" + index + ", cells=" + cells.Count + ")";
        }
    }
}
