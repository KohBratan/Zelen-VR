// ---------------------------------------------------------------------------
// Zelen-VR
// File: GridUtils.cs
// Utility setki.
// ---------------------------------------------------------------------------
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Small stateless helpers used across the grid modules. The class avoids
    /// allocations on hot paths where possible and never mutates its inputs.
    /// </summary>
    public static class GridUtils
    {
        public const float Epsilon = 0.0001f;

        public static bool IsPowerOfTwo(int value)
        {
            return value > 0 && (value & (value - 1)) == 0;
        }

        public static int NextPowerOfTwo(int value)
        {
            if (value <= 1)
            {
                return 1;
            }
            int result = 1;
            while (result < value)
            {
                result <<= 1;
            }
            return result;
        }

        public static float SafeDivide(float numerator, float denominator, float fallback = 0f)
        {
            if (Mathf.Abs(denominator) <= Epsilon)
            {
                return fallback;
            }
            return numerator / denominator;
        }

        public static int Wrap(int value, int minInclusive, int maxExclusive)
        {
            int range = maxExclusive - minInclusive;
            if (range <= 0)
            {
                return minInclusive;
            }
            int offset = (value - minInclusive) % range;
            if (offset < 0)
            {
                offset += range;
            }
            return minInclusive + offset;
        }

        public static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        public static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        public static string FormatCoordinate(GridCoordinate coordinate)
        {
            return "[" + coordinate.X + ", " + coordinate.Y + "]";
        }

        public static string FormatBounds(GridBounds bounds)
        {
            return bounds.IsValid ? bounds.ToString() : "GridBounds[empty]";
        }

        public static int CountRect(GridCoordinate min, GridCoordinate max)
        {
            int width = Mathf.Max(0, max.X - min.X + 1);
            int height = Mathf.Max(0, max.Y - min.Y + 1);
            return width * height;
        }

        public static IEnumerable<GridCoordinate> EnumerateRect(GridCoordinate min, GridCoordinate max)
        {
            int x0 = Mathf.Min(min.X, max.X);
            int y0 = Mathf.Min(min.Y, max.Y);
            int x1 = Mathf.Max(min.X, max.X);
            int y1 = Mathf.Max(min.Y, max.Y);
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    yield return new GridCoordinate(x, y);
                }
            }
        }

        public static IEnumerable<GridCoordinate> EnumerateBorder(GridCoordinate min, GridCoordinate max)
        {
            int x0 = Mathf.Min(min.X, max.X);
            int y0 = Mathf.Min(min.Y, max.Y);
            int x1 = Mathf.Max(min.X, max.X);
            int y1 = Mathf.Max(min.Y, max.Y);

            for (int x = x0; x <= x1; x++)
            {
                yield return new GridCoordinate(x, y0);
                if (y1 != y0)
                {
                    yield return new GridCoordinate(x, y1);
                }
            }
            for (int y = y0 + 1; y < y1; y++)
            {
                yield return new GridCoordinate(x0, y);
                if (x1 != x0)
                {
                    yield return new GridCoordinate(x1, y);
                }
            }
        }

        public static string DescribeCells(IReadOnlyList<GridCoordinate> cells, int limit = 16)
        {
            if (cells == null || cells.Count == 0)
            {
                return "none";
            }

            StringBuilder builder = new StringBuilder();
            int count = Mathf.Min(cells.Count, Mathf.Max(0, limit));
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(FormatCoordinate(cells[i]));
            }
            if (cells.Count > count)
            {
                builder.Append(" ... (+");
                builder.Append(cells.Count - count);
                builder.Append(")");
            }
            return builder.ToString();
        }
    }

    /// <summary>Convenience extensions for the integer grid coordinate type.</summary>
    public static class GridCoordinateExtensions
    {
        public static bool IsZero(this GridCoordinate coordinate)
        {
            return coordinate.X == 0 && coordinate.Y == 0;
        }

        public static GridCoordinate Abs(this GridCoordinate coordinate)
        {
            return new GridCoordinate(Mathf.Abs(coordinate.X), Mathf.Abs(coordinate.Y));
        }

        public static GridCoordinate Clamp(this GridCoordinate coordinate, GridCoordinate min, GridCoordinate max)
        {
            return new GridCoordinate(
                Mathf.Clamp(coordinate.X, min.X, max.X),
                Mathf.Clamp(coordinate.Y, min.Y, max.Y));
        }

        public static Vector2 ToVector2(this GridCoordinate coordinate)
        {
            return new Vector2(coordinate.X, coordinate.Y);
        }

        public static Vector3Int ToVector3Int(this GridCoordinate coordinate)
        {
            return new Vector3Int(coordinate.X, coordinate.Y, 0);
        }
    }

    /// <summary>Convenience extensions for Unity vector and color types.</summary>
    public static class GridValueExtensions
    {
        public static Vector3 WithY(this Vector3 value, float y)
        {
            return new Vector3(value.x, y, value.z);
        }

        public static Vector3 FlattenToPlane(this Vector3 value, GridPlane plane)
        {
            switch (plane)
            {
                case GridPlane.XY:
                    return new Vector3(value.x, value.y, 0f);
                case GridPlane.YZ:
                    return new Vector3(0f, value.y, value.z);
                default:
                    return new Vector3(value.x, 0f, value.z);
            }
        }

        public static bool IsFinite(this Vector3 value)
        {
            return GridUtils.IsFinite(value);
        }

        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
    }
}
