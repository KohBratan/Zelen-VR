using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>Supported brush silhouettes.</summary>
    public enum GridBrushShape
    {
        Square = 0,
        Circle = 1,
        Diamond = 2,
        Cross = 3
    }

    /// <summary>
    /// Describes the footprint of a brush in cell units. The brush is purely
    /// data: the rasterizer asks for offsets and applies them around a centre
    /// coordinate. Size 1 produces exactly one cell.
    /// </summary>
    [Serializable]
    public struct GridBrush
    {
        [SerializeField] private GridBrushShape shape;
        [SerializeField, Min(1)] private int size;
        [SerializeField] private bool filled;

        public GridBrushShape Shape { get { return shape; } set { shape = value; } }
        public int Size { get { return Mathf.Max(1, size); } set { size = Mathf.Max(1, value); } }
        public bool Filled { get { return filled; } set { filled = value; } }

        public GridBrush(GridBrushShape shape, int size, bool filled)
        {
            this.shape = shape;
            this.size = Mathf.Max(1, size);
            this.filled = filled;
        }

        public static GridBrush Single
        {
            get { return new GridBrush(GridBrushShape.Square, 1, true); }
        }

        public static GridBrush Square(int size, bool filled = true)
        {
            return new GridBrush(GridBrushShape.Square, size, filled);
        }

        public static GridBrush Circle(int size, bool filled = true)
        {
            return new GridBrush(GridBrushShape.Circle, size, filled);
        }

        /// <summary>Enumerates the cell offsets covered by the brush.</summary>
        public IEnumerable<GridCoordinate> GetOffsets()
        {
            int radius = Size - 1;
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (!IsInside(x, y, radius))
                    {
                        continue;
                    }
                    yield return new GridCoordinate(x, y);
                }
            }
        }

        private bool IsInside(int x, int y, int radius)
        {
            switch (shape)
            {
                case GridBrushShape.Circle:
                    if (!filled)
                    {
                        float distance = Mathf.Sqrt(x * x + y * y);
                        return distance <= radius + 0.5f && distance >= radius - 0.5f;
                    }
                    return (x * x + y * y) <= (radius + 0.5f) * (radius + 0.5f);
                case GridBrushShape.Diamond:
                    if (!filled)
                    {
                        return Mathf.Abs(x) + Mathf.Abs(y) == radius;
                    }
                    return Mathf.Abs(x) + Mathf.Abs(y) <= radius;
                case GridBrushShape.Cross:
                    return x == 0 || y == 0;
                default:
                    if (!filled)
                    {
                        return Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) == radius;
                    }
                    return true;
            }
        }

        /// <summary>Bounding rectangle of the brush footprint.</summary>
        public GridBounds GetBounds(GridCoordinate centre)
        {
            int radius = Size - 1;
            return new GridBounds(
                centre.X - radius,
                centre.Y - radius,
                centre.X + radius,
                centre.Y + radius);
        }

        public override string ToString()
        {
            return "GridBrush(" + shape + ", size=" + Size + ", filled=" + filled + ")";
        }
    }
}
