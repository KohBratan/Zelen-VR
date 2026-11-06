using System;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Stateless conversion helpers between world space and the discrete grid.
    /// Every method is pure and only reads the supplied settings asset, which
    /// makes the maths safe to unit test outside of play mode.
    /// </summary>
    public static class GridMath
    {
        public const float Epsilon = 0.0001f;

        /// <summary>World position of the lower corner of cell (0,0).</summary>
        public static Vector3 GetCellZeroCorner(GridSettings settings)
        {
            if (settings == null)
            {
                return Vector3.zero;
            }

            Vector3 origin = settings.Origin;
            if (settings.OriginMode == GridOriginMode.Center)
            {
                origin -= HalfGridExtent(settings);
            }
            return origin;
        }

        /// <summary>Half of the total grid extent expressed in world units on the grid plane.</summary>
        public static Vector3 HalfGridExtent(GridSettings settings)
        {
            if (settings == null)
            {
                return Vector3.zero;
            }

            float cs = settings.CellSize;
            float halfWidth = settings.Width * cs * 0.5f;
            float halfHeight = settings.Height * cs * 0.5f;
            switch (settings.Plane)
            {
                case GridPlane.XY:
                    return new Vector3(halfWidth, halfHeight, 0f);
                case GridPlane.YZ:
                    return new Vector3(0f, halfHeight, halfWidth);
                default:
                    return new Vector3(halfWidth, 0f, halfHeight);
            }
        }

        /// <summary>World position of the lower corner of the given cell.</summary>
        public static Vector3 GridToWorld(GridCoordinate coordinate, GridSettings settings)
        {
            Vector3 corner = GetCellZeroCorner(settings);
            if (settings == null)
            {
                return corner;
            }

            float cs = settings.CellSize;
            switch (settings.Plane)
            {
                case GridPlane.XY:
                    return corner + new Vector3(coordinate.X * cs, coordinate.Y * cs, 0f);
                case GridPlane.YZ:
                    return corner + new Vector3(0f, coordinate.Y * cs, coordinate.X * cs);
                default:
                    return corner + new Vector3(coordinate.X * cs, 0f, coordinate.Y * cs);
            }
        }

        /// <summary>World position of the geometric centre of the given cell.</summary>
        public static Vector3 CellCenter(GridCoordinate coordinate, GridSettings settings)
        {
            return GridToWorld(coordinate, settings) + HalfCellOffset(settings);
        }

        /// <summary>Offset from the lower cell corner to its centre.</summary>
        public static Vector3 HalfCellOffset(GridSettings settings)
        {
            if (settings == null)
            {
                return Vector3.zero;
            }

            float half = settings.CellSize * 0.5f;
            switch (settings.Plane)
            {
                case GridPlane.XY:
                    return new Vector3(half, half, 0f);
                case GridPlane.YZ:
                    return new Vector3(0f, half, half);
                default:
                    return new Vector3(half, 0f, half);
            }
        }

        /// <summary>Converts a world position into the grid cell that contains it.</summary>
        public static GridCoordinate WorldToGrid(Vector3 world, GridSettings settings, bool clamp = true)
        {
            if (settings == null)
            {
                return GridCoordinate.Zero;
            }

            Vector3 local = world - GetCellZeroCorner(settings);
            float cs = settings.CellSize;
            int x;
            int y;
            switch (settings.Plane)
            {
                case GridPlane.XY:
                    x = Mathf.FloorToInt(local.x / cs);
                    y = Mathf.FloorToInt(local.y / cs);
                    break;
                case GridPlane.YZ:
                    x = Mathf.FloorToInt(local.z / cs);
                    y = Mathf.FloorToInt(local.y / cs);
                    break;
                default:
                    x = Mathf.FloorToInt(local.x / cs);
                    y = Mathf.FloorToInt(local.z / cs);
                    break;
            }

            GridCoordinate coordinate = new GridCoordinate(x, y);
            return clamp ? Clamp(coordinate, settings) : coordinate;
        }

        /// <summary>Converts a world position into the nearest grid cell (rounded, not floored).</summary>
        public static GridCoordinate WorldToGridRounded(Vector3 world, GridSettings settings, bool clamp = true)
        {
            if (settings == null)
            {
                return GridCoordinate.Zero;
            }

            Vector3 local = world - GetCellZeroCorner(settings);
            float cs = settings.CellSize;
            int x;
            int y;
            switch (settings.Plane)
            {
                case GridPlane.XY:
                    x = Mathf.RoundToInt(local.x / cs);
                    y = Mathf.RoundToInt(local.y / cs);
                    break;
                case GridPlane.YZ:
                    x = Mathf.RoundToInt(local.z / cs);
                    y = Mathf.RoundToInt(local.y / cs);
                    break;
                default:
                    x = Mathf.RoundToInt(local.x / cs);
                    y = Mathf.RoundToInt(local.z / cs);
                    break;
            }

            GridCoordinate coordinate = new GridCoordinate(x, y);
            return clamp ? Clamp(coordinate, settings) : coordinate;
        }

        /// <summary>Clamps a coordinate into the valid grid range.</summary>
        public static GridCoordinate Clamp(GridCoordinate coordinate, GridSettings settings)
        {
            if (settings == null)
            {
                return coordinate;
            }
            int maxX = Mathf.Max(0, settings.Width - 1);
            int maxY = Mathf.Max(0, settings.Height - 1);
            return new GridCoordinate(Mathf.Clamp(coordinate.X, 0, maxX), Mathf.Clamp(coordinate.Y, 0, maxY));
        }

        /// <summary>True when the coordinate lies inside the grid bounds.</summary>
        public static bool Contains(GridCoordinate coordinate, GridSettings settings)
        {
            if (settings == null)
            {
                return false;
            }
            return coordinate.X >= 0 && coordinate.Y >= 0 && coordinate.X < settings.Width && coordinate.Y < settings.Height;
        }

        /// <summary>Projects a world position orthogonally onto the grid plane.</summary>
        public static Vector3 ProjectOntoPlane(Vector3 world, GridSettings settings)
        {
            if (settings == null)
            {
                return world;
            }
            Vector3 corner = GetCellZeroCorner(settings);
            Vector3 normal = settings.PlaneNormal;
            float distance = Vector3.Dot(world - corner, normal);
            return world - normal * distance;
        }

        /// <summary>Snaps a world position to the centre of the nearest cell.</summary>
        public static Vector3 SnapToCellCenter(Vector3 world, GridSettings settings)
        {
            GridCoordinate coordinate = WorldToGrid(world, settings, true);
            return CellCenter(coordinate, settings);
        }

        /// <summary>Rounds a world position to the nearest cell corner.</summary>
        public static Vector3 SnapToCellCorner(Vector3 world, GridSettings settings)
        {
            GridCoordinate coordinate = WorldToGridRounded(world, settings, true);
            return GridToWorld(coordinate, settings);
        }

        /// <summary>Chunk index that owns the given cell.</summary>
        public static Vector2Int CellToChunk(GridCoordinate coordinate, GridSettings settings)
        {
            if (settings == null)
            {
                return Vector2Int.zero;
            }
            int size = settings.ChunkSize;
            int cx = coordinate.X >= 0 ? coordinate.X / size : (coordinate.X - size + 1) / size;
            int cy = coordinate.Y >= 0 ? coordinate.Y / size : (coordinate.Y - size + 1) / size;
            return new Vector2Int(cx, cy);
        }

        /// <summary>Number of chunks along the horizontal axis.</summary>
        public static int ChunkCountX(GridSettings settings)
        {
            if (settings == null)
            {
                return 0;
            }
            return Mathf.CeilToInt((float)settings.Width / settings.ChunkSize);
        }

        /// <summary>Number of chunks along the vertical axis.</summary>
        public static int ChunkCountY(GridSettings settings)
        {
            if (settings == null)
            {
                return 0;
            }
            return Mathf.CeilToInt((float)settings.Height / settings.ChunkSize);
        }

        /// <summary>Interpolates between two coordinates and rounds the result.</summary>
        public static GridCoordinate Lerp(GridCoordinate a, GridCoordinate b, float t)
        {
            return new GridCoordinate(
                Mathf.RoundToInt(Mathf.Lerp(a.X, b.X, t)),
                Mathf.RoundToInt(Mathf.Lerp(a.Y, b.Y, t)));
        }

        /// <summary>Euclidean distance in cell units.</summary>
        public static float Distance(GridCoordinate a, GridCoordinate b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>True when the two coordinates are neighbours (including diagonals).</summary>
        public static bool AreNeighbours(GridCoordinate a, GridCoordinate b)
        {
            int dx = Mathf.Abs(a.X - b.X);
            int dy = Mathf.Abs(a.Y - b.Y);
            return dx <= 1 && dy <= 1 && (dx + dy) > 0;
        }
    }
}
