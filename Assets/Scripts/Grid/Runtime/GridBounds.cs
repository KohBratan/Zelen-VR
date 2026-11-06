using System;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Inclusive integer rectangle covering a set of grid coordinates. The
    /// struct is used for dirty regions, rendering culling and serialization.
    /// </summary>
    [Serializable]
    public struct GridBounds : IEquatable<GridBounds>
    {
        [SerializeField] private int minX;
        [SerializeField] private int minY;
        [SerializeField] private int maxX;
        [SerializeField] private int maxY;

        public static readonly GridBounds Empty = new GridBounds(0, 0, -1, -1);

        public int MinX { get { return minX; } }
        public int MinY { get { return minY; } }
        public int MaxX { get { return maxX; } }
        public int MaxY { get { return maxY; } }

        public int Width { get { return IsValid ? maxX - minX + 1 : 0; } }
        public int Height { get { return IsValid ? maxY - minY + 1 : 0; } }
        public int Area { get { return Width * Height; } }
        public bool IsValid { get { return maxX >= minX && maxY >= minY; } }

        public GridBounds(int minX, int minY, int maxX, int maxY)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
        }

        public GridBounds(GridCoordinate min, GridCoordinate max)
        {
            minX = min.X;
            minY = min.Y;
            maxX = max.X;
            maxY = max.Y;
        }

        public GridCoordinate Min { get { return new GridCoordinate(minX, minY); } }
        public GridCoordinate Max { get { return new GridCoordinate(maxX, maxY); } }

        public static GridBounds FromCoordinates(GridCoordinate a, GridCoordinate b)
        {
            return new GridBounds(
                Mathf.Min(a.X, b.X),
                Mathf.Min(a.Y, b.Y),
                Mathf.Max(a.X, b.X),
                Mathf.Max(a.Y, b.Y));
        }

        public static GridBounds FromCenter(GridCoordinate center, int radius)
        {
            int r = Mathf.Max(0, radius);
            return new GridBounds(center.X - r, center.Y - r, center.X + r, center.Y + r);
        }

        public bool Contains(GridCoordinate coordinate)
        {
            return IsValid
                && coordinate.X >= minX && coordinate.X <= maxX
                && coordinate.Y >= minY && coordinate.Y <= maxY;
        }

        public bool Contains(GridBounds other)
        {
            return IsValid && other.IsValid
                && other.minX >= minX && other.maxX <= maxX
                && other.minY >= minY && other.maxY <= maxY;
        }

        public bool Overlaps(GridBounds other)
        {
            if (!IsValid || !other.IsValid)
            {
                return false;
            }
            return minX <= other.maxX && maxX >= other.minX
                && minY <= other.maxY && maxY >= other.minY;
        }

        public GridBounds Intersect(GridBounds other)
        {
            if (!Overlaps(other))
            {
                return Empty;
            }
            return new GridBounds(
                Mathf.Max(minX, other.minX),
                Mathf.Max(minY, other.minY),
                Mathf.Min(maxX, other.maxX),
                Mathf.Min(maxY, other.maxY));
        }

        public void Encapsulate(GridCoordinate coordinate)
        {
            if (!IsValid)
            {
                minX = maxX = coordinate.X;
                minY = maxY = coordinate.Y;
                return;
            }

            minX = Mathf.Min(minX, coordinate.X);
            minY = Mathf.Min(minY, coordinate.Y);
            maxX = Mathf.Max(maxX, coordinate.X);
            maxY = Mathf.Max(maxY, coordinate.Y);
        }

        public void Encapsulate(GridBounds other)
        {
            if (!other.IsValid)
            {
                return;
            }
            Encapsulate(other.Min);
            Encapsulate(other.Max);
        }

        public GridBounds Expanded(int amount)
        {
            int a = Mathf.Max(0, amount);
            return new GridBounds(minX - a, minY - a, maxX + a, maxY + a);
        }

        public GridBounds ClampedTo(GridSettings settings)
        {
            if (!IsValid || settings == null)
            {
                return Empty;
            }

            return new GridBounds(
                Mathf.Clamp(minX, 0, settings.Width - 1),
                Mathf.Clamp(minY, 0, settings.Height - 1),
                Mathf.Clamp(maxX, 0, settings.Width - 1),
                Mathf.Clamp(maxY, 0, settings.Height - 1));
        }

        public bool Equals(GridBounds other)
        {
            return minX == other.minX && minY == other.minY
                && maxX == other.maxX && maxY == other.maxY;
        }

        public override bool Equals(object obj)
        {
            return obj is GridBounds && Equals((GridBounds)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = minX;
                hash = (hash * 397) ^ minY;
                hash = (hash * 397) ^ maxX;
                hash = (hash * 397) ^ maxY;
                return hash;
            }
        }

        public override string ToString()
        {
            return "GridBounds[" + minX + "," + minY + " -> " + maxX + "," + maxY + "]";
        }
    }
}
