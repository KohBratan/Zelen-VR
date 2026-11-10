using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Scene-view gizmo helper for inspecting grid geometry and highlighted
    /// cells. It never changes runtime state and is safe to leave on prefabs.
    /// </summary>
    [DisallowMultipleComponent]
    public class GridDebugDrawer : MonoBehaviour
    {
        [SerializeField] private GridSettings settings;
        [SerializeField] private bool drawGrid = true;
        [SerializeField] private bool drawCellCenters = false;
        [SerializeField] private Color gridColor = new Color(0.3f, 0.9f, 1f, 0.5f);
        [SerializeField] private Color highlightColor = new Color(1f, 0.75f, 0.1f, 0.8f);
        [SerializeField] private List<GridCoordinate> highlightedCells = new List<GridCoordinate>();

        public GridSettings Settings { get { return settings; } }

        public void SetSettings(GridSettings newSettings)
        {
            settings = newSettings;
        }

        public void Highlight(GridCoordinate coordinate)
        {
            if (!highlightedCells.Contains(coordinate))
            {
                highlightedCells.Add(coordinate);
            }
        }

        public void ClearHighlights()
        {
            highlightedCells.Clear();
        }

        private void OnDrawGizmos()
        {
            if (settings == null)
            {
                return;
            }

            if (drawGrid)
            {
                DrawGridGizmos();
            }

            if (drawCellCenters)
            {
                DrawCellCenters();
            }

            Gizmos.color = highlightColor;
            for (int i = 0; i < highlightedCells.Count; i++)
            {
                DrawCell(highlightedCells[i], highlightColor);
            }
        }

        private void DrawGridGizmos()
        {
            Gizmos.color = gridColor;
            int width = settings.Width;
            int height = settings.Height;

            for (int y = 0; y <= height; y++)
            {
                Vector3 a = GridMath.GridToWorld(new GridCoordinate(0, y), settings);
                Vector3 b = GridMath.GridToWorld(new GridCoordinate(width, y), settings);
                Gizmos.DrawLine(a, b);
            }

            for (int x = 0; x <= width; x++)
            {
                Vector3 a = GridMath.GridToWorld(new GridCoordinate(x, 0), settings);
                Vector3 b = GridMath.GridToWorld(new GridCoordinate(x, height), settings);
                Gizmos.DrawLine(a, b);
            }
        }

        private void DrawCellCenters()
        {
            Gizmos.color = highlightColor;
            for (int y = 0; y < settings.Height; y++)
            {
                for (int x = 0; x < settings.Width; x++)
                {
                    Vector3 center = GridMath.CellCenter(new GridCoordinate(x, y), settings);
                    Gizmos.DrawSphere(center, settings.CellSize * 0.04f);
                }
            }
        }

        private void DrawCell(GridCoordinate coordinate, Color color)
        {
            if (!settings.Contains(coordinate))
            {
                return;
            }

            Vector3 center = GridMath.CellCenter(coordinate, settings);
            Vector3 size = CellWorldSize();
            Gizmos.color = color;
            Gizmos.DrawWireCube(center, size);
        }

        private Vector3 CellWorldSize()
        {
            float cs = settings.CellSize;
            switch (settings.Plane)
            {
                case GridPlane.XY:
                    return new Vector3(cs, cs, 0.001f);
                case GridPlane.YZ:
                    return new Vector3(0.001f, cs, cs);
                default:
                    return new Vector3(cs, 0.001f, cs);
            }
        }
    }
}
