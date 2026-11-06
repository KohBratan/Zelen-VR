using System;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Discrete integer coordinate of a single grid cell on the grid plane.
    /// The X axis maps to the horizontal plane axis, Y to the vertical one.
    /// </summary>
    [Serializable]
    public struct GridCoordinate : IEquatable<GridCoordinate>
    {
        [SerializeField] private int x;
        [SerializeField] private int y;

        public int X { get { return x; } set { x = value; } }
        public int Y { get { return y; } set { y = value; } }

        public static readonly GridCoordinate Zero = new GridCoordinate(0, 0);
        public static readonly GridCoordinate One = new GridCoordinate(1, 1);
        public static readonly GridCoordinate Up = new GridCoordinate(0, 1);
        public static readonly GridCoordinate Right = new GridCoordinate(1, 0);
        public static readonly GridCoordinate Down = new GridCoordinate(0, -1);
        public static readonly GridCoordinate Left = new GridCoordinate(-1, 0);

        public GridCoordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public GridCoordinate(Vector2Int vector)
        {
            x = vector.x;
            y = vector.y;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }

        public static GridCoordinate FromVector2Int(Vector2Int vector)
        {
            return new GridCoordinate(vector.x, vector.y);
        }

        /// <summary>Taxicab distance measured in cells.</summary>
        public int ManhattanDistance(GridCoordinate other)
        {
            return Mathf.Abs(x - other.x) + Mathf.Abs(y - other.y);
        }

        /// <summary>Chebyshev distance measured in cells (king moves).</summary>
        public int ChebyshevDistance(GridCoordinate other)
        {
            return Mathf.Max(Mathf.Abs(x - other.x), Mathf.Abs(y - other.y));
        }

        public static GridCoordinate operator +(GridCoordinate a, GridCoordinate b)
        {
            return new GridCoordinate(a.x + b.x, a.y + b.y);
        }

        public static GridCoordinate operator -(GridCoordinate a, GridCoordinate b)
        {
            return new GridCoordinate(a.x - b.x, a.y - b.y);
        }

        public static GridCoordinate operator -(GridCoordinate a)
        {
            return new GridCoordinate(-a.x, -a.y);
        }

        public static GridCoordinate operator *(GridCoordinate a, int scale)
        {
            return new GridCoordinate(a.x * scale, a.y * scale);
        }

        public static GridCoordinate operator *(int scale, GridCoordinate a)
        {
            return new GridCoordinate(a.x * scale, a.y * scale);
        }

        public static bool operator ==(GridCoordinate a, GridCoordinate b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(GridCoordinate a, GridCoordinate b)
        {
            return !(a == b);
        }

        public bool Equals(GridCoordinate other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCoordinate && Equals((GridCoordinate)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x * 397) ^ y;
            }
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }
}
