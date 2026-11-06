using System;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>Plane on which the two-dimensional grid is laid out.</summary>
    public enum GridPlane
    {
        XY = 0,
        XZ = 1,
        YZ = 2
    }

    /// <summary>How the origin transform relates to the grid rectangle.</summary>
    public enum GridOriginMode
    {
        Corner = 0,
        Center = 1
    }

    /// <summary>
    /// Immutable-by-convention configuration asset for a grid. Create instances
    /// through the Assets/Create/ZelenVR/Grid menu. All runtime components read
    /// their dimensions from here so a single asset can drive many grids.
    /// </summary>
    [CreateAssetMenu(fileName = "GridSettings", menuName = "ZelenVR/Grid/Grid Settings", order = 0)]
    public class GridSettings : ScriptableObject
    {
        public const int MinDimension = 1;
        public const int MaxDimension = 4096;
        public const float MinCellSize = 0.01f;
        public const float MaxCellSize = 10f;

        [Header("Dimensions")]
        [SerializeField, Min(MinDimension)] private int width = 32;
        [SerializeField, Min(MinDimension)] private int height = 32;
        [SerializeField, Min(MinCellSize)] private float cellSize = 0.25f;

        [Header("Placement")]
        [SerializeField] private GridPlane plane = GridPlane.XZ;
        [SerializeField] private GridOriginMode originMode = GridOriginMode.Corner;
        [SerializeField] private Vector3 origin = Vector3.zero;
        [SerializeField, Min(1)] private int chunkSize = 16;

        [Header("Runtime")]
        [SerializeField] private bool drawInEditMode = false;
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.25f);

        public int Width
        {
            get { return width; }
            set { width = Mathf.Clamp(value, MinDimension, MaxDimension); }
        }

        public int Height
        {
            get { return height; }
            set { height = Mathf.Clamp(value, MinDimension, MaxDimension); }
        }

        public float CellSize
        {
            get { return cellSize; }
            set { cellSize = Mathf.Clamp(value, MinCellSize, MaxCellSize); }
        }

        public GridPlane Plane
        {
            get { return plane; }
            set { plane = value; }
        }

        public GridOriginMode OriginMode
        {
            get { return originMode; }
            set { originMode = value; }
        }

        public Vector3 Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        public int ChunkSize
        {
            get { return Mathf.Max(1, chunkSize); }
            set { chunkSize = Mathf.Max(1, value); }
        }

        public bool DrawInEditMode
        {
            get { return drawInEditMode; }
            set { drawInEditMode = value; }
        }

        public Color GridColor
        {
            get { return gridColor; }
            set { gridColor = value; }
        }

        public int CellCount { get { return width * height; } }

        public Vector2 CellSizeVector2 { get { return new Vector2(cellSize, cellSize); } }

        public int ChunkCountX { get { return Mathf.CeilToInt((float)width / ChunkSize); } }
        public int ChunkCountY { get { return Mathf.CeilToInt((float)height / ChunkSize); } }

        public Vector3 PlaneNormal
        {
            get
            {
                switch (plane)
                {
                    case GridPlane.XY: return Vector3.forward;
                    case GridPlane.YZ: return Vector3.right;
                    default: return Vector3.up;
                }
            }
        }

        public Vector3 PlaneRight
        {
            get
            {
                switch (plane)
                {
                    case GridPlane.XY: return Vector3.right;
                    case GridPlane.YZ: return Vector3.forward;
                    default: return Vector3.right;
                }
            }
        }

        public Vector3 PlaneUp
        {
            get
            {
                switch (plane)
                {
                    case GridPlane.XY: return Vector3.up;
                    case GridPlane.YZ: return Vector3.up;
                    default: return Vector3.forward;
                }
            }
        }

        public bool Contains(GridCoordinate coordinate)
        {
            return coordinate.X >= 0 && coordinate.Y >= 0 && coordinate.X < width && coordinate.Y < height;
        }

        public void CopyFrom(GridSettings other)
        {
            if (other == null)
            {
                return;
            }
            width = other.width;
            height = other.height;
            cellSize = other.cellSize;
            plane = other.plane;
            originMode = other.originMode;
            origin = other.origin;
            chunkSize = other.chunkSize;
            drawInEditMode = other.drawInEditMode;
            gridColor = other.gridColor;
        }

        private void OnValidate()
        {
            width = Mathf.Clamp(width, MinDimension, MaxDimension);
            height = Mathf.Clamp(height, MinDimension, MaxDimension);
            cellSize = Mathf.Clamp(cellSize, MinCellSize, MaxCellSize);
            chunkSize = Mathf.Max(1, chunkSize);
        }
    }
}
