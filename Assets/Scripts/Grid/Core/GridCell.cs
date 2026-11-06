using System;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Logical state of a cell after rasterization or an edit operation.
    /// </summary>
    public enum GridCellState
    {
        Empty = 0,
        Painted = 1,
        Erased = 2,
        Locked = 3
    }

    /// <summary>
    /// Value type describing one cell on a grid layer. Instances are cheap to
    /// copy; use the With* helpers to derive modified variants without mutating
    /// shared data.
    /// </summary>
    [Serializable]
    public struct GridCell : IEquatable<GridCell>
    {
        [SerializeField] private GridCoordinate coordinate;
        [SerializeField] private GridCellState state;
        [SerializeField] private int layer;
        [SerializeField] private int palette;
        [SerializeField] private float fill;

        public GridCoordinate Coordinate { get { return coordinate; } }
        public GridCellState State { get { return state; } }
        public int Layer { get { return layer; } }
        public int Palette { get { return palette; } }
        public float Fill { get { return fill; } }

        public bool IsOccupied
        {
            get { return state == GridCellState.Painted || state == GridCellState.Locked; }
        }

        public bool IsEmpty
        {
            get { return state == GridCellState.Empty; }
        }

        public GridCell(GridCoordinate coordinate, GridCellState state, int layer, int palette, float fill)
        {
            this.coordinate = coordinate;
            this.state = state;
            this.layer = layer;
            this.palette = palette;
            this.fill = Mathf.Clamp01(fill);
        }

        public static GridCell CreateEmpty(GridCoordinate coordinate)
        {
            return new GridCell(coordinate, GridCellState.Empty, 0, 0, 0f);
        }

        public static GridCell CreatePainted(GridCoordinate coordinate, int layer, int palette)
        {
            return new GridCell(coordinate, GridCellState.Painted, layer, palette, 1f);
        }

        public static GridCell CreateErased(GridCoordinate coordinate, int layer)
        {
            return new GridCell(coordinate, GridCellState.Erased, layer, 0, 0f);
        }

        public static GridCell CreateLocked(GridCoordinate coordinate, int layer, int palette)
        {
            return new GridCell(coordinate, GridCellState.Locked, layer, palette, 1f);
        }

        public GridCell WithState(GridCellState newState)
        {
            return new GridCell(coordinate, newState, layer, palette, fill);
        }

        public GridCell WithLayer(int newLayer)
        {
            return new GridCell(coordinate, state, newLayer, palette, fill);
        }

        public GridCell WithPalette(int newPalette)
        {
            return new GridCell(coordinate, state, layer, newPalette, fill);
        }

        public GridCell WithFill(float newFill)
        {
            return new GridCell(coordinate, state, layer, palette, newFill);
        }

        public bool Equals(GridCell other)
        {
            return coordinate == other.coordinate
                && state == other.state
                && layer == other.layer
                && palette == other.palette
                && Mathf.Approximately(fill, other.fill);
        }

        public override bool Equals(object obj)
        {
            return obj is GridCell && Equals((GridCell)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = coordinate.GetHashCode();
                hash = (hash * 397) ^ (int)state;
                hash = (hash * 397) ^ layer;
                hash = (hash * 397) ^ palette;
                hash = (hash * 397) ^ fill.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return "GridCell" + coordinate + " state=" + state + " layer=" + layer + " palette=" + palette;
        }
    }
}
