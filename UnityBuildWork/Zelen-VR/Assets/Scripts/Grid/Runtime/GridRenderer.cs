// ---------------------------------------------------------------------------
// Zelen-VR
// File: GridRenderer.cs
// Render setki.
// ---------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Draws the grid lines with lightweight LineRenderer objects. The renderer
    /// is deliberately independent from the manager's cell data: it only shows
    /// the regular lattice and can be toggled without touching the simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public class GridRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager manager;
        [SerializeField] private GridSettings settings;
        [SerializeField] private Material lineMaterial;

        [Header("Appearance")]
        [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 0.22f);
        [SerializeField] private float lineWidth = 0.01f;
        [SerializeField] private bool drawBorder = true;
        [SerializeField] private bool useWorldSpace = true;

        private readonly List<LineRenderer> lines = new List<LineRenderer>();
        private Transform lineRoot;
        private bool dirty = true;

        public GridSettings Settings { get { return settings; } }

        private void Awake()
        {
            if (manager == null)
            {
                manager = GetComponent<GridManager>();
            }
            if (manager != null && settings == null)
            {
                settings = manager.Settings;
            }
        }

        private void OnEnable()
        {
            if (manager != null)
            {
                manager.GridInitialized += HandleGridInitialized;
                manager.GridCleared += MarkDirty;
            }
            MarkDirty();
        }

        private void OnDisable()
        {
            if (manager != null)
            {
                manager.GridInitialized -= HandleGridInitialized;
                manager.GridCleared -= MarkDirty;
            }
        }

        private void LateUpdate()
        {
            if (dirty)
            {
                Rebuild();
            }
        }

        public void SetSettings(GridSettings newSettings)
        {
            settings = newSettings;
            MarkDirty();
        }

        public void MarkDirty()
        {
            dirty = true;
        }

        private void HandleGridInitialized(GridSettings initializedSettings)
        {
            settings = initializedSettings;
            MarkDirty();
        }

        /// <summary>Destroys previously generated line objects and rebuilds them.</summary>
        public void Rebuild()
        {
            ClearLines();
            dirty = false;

            if (settings == null)
            {
                return;
            }

            EnsureRoot();
            BuildHorizontalLines();
            BuildVerticalLines();
            if (drawBorder)
            {
                BuildBorder();
            }
        }

        private void EnsureRoot()
        {
            if (lineRoot != null)
            {
                return;
            }

            GameObject root = new GameObject("GridLines");
            root.transform.SetParent(transform, false);
            root.hideFlags = HideFlags.DontSave;
            lineRoot = root.transform;
        }

        private void BuildHorizontalLines()
        {
            int width = settings.Width;
            int height = settings.Height;
            for (int y = 0; y <= height; y++)
            {
                if (!drawBorder && (y == 0 || y == height))
                {
                    continue;
                }
                GridCoordinate a = new GridCoordinate(0, y);
                GridCoordinate b = new GridCoordinate(width, y);
                CreateLine(GridMath.GridToWorld(a, settings), GridMath.GridToWorld(b, settings));
            }
        }

        private void BuildVerticalLines()
        {
            int width = settings.Width;
            int height = settings.Height;
            for (int x = 0; x <= width; x++)
            {
                if (!drawBorder && (x == 0 || x == width))
                {
                    continue;
                }
                GridCoordinate a = new GridCoordinate(x, 0);
                GridCoordinate b = new GridCoordinate(x, height);
                CreateLine(GridMath.GridToWorld(a, settings), GridMath.GridToWorld(b, settings));
            }
        }

        private void BuildBorder()
        {
            Vector3 bottomLeft = GridMath.GridToWorld(new GridCoordinate(0, 0), settings);
            Vector3 bottomRight = GridMath.GridToWorld(new GridCoordinate(settings.Width, 0), settings);
            Vector3 topRight = GridMath.GridToWorld(new GridCoordinate(settings.Width, settings.Height), settings);
            Vector3 topLeft = GridMath.GridToWorld(new GridCoordinate(0, settings.Height), settings);

            CreateLine(bottomLeft, bottomRight);
            CreateLine(bottomRight, topRight);
            CreateLine(topRight, topLeft);
            CreateLine(topLeft, bottomLeft);
        }

        private void CreateLine(Vector3 start, Vector3 end)
        {
            GameObject go = new GameObject("GridLine");
            go.transform.SetParent(lineRoot, false);
            go.hideFlags = HideFlags.DontSave;

            LineRenderer renderer = go.AddComponent<LineRenderer>();
            renderer.useWorldSpace = useWorldSpace;
            renderer.positionCount = 2;
            renderer.SetPosition(0, start);
            renderer.SetPosition(1, end);
            renderer.startWidth = lineWidth;
            renderer.endWidth = lineWidth;
            renderer.startColor = lineColor;
            renderer.endColor = lineColor;
            renderer.numCapVertices = 0;
            renderer.numCornerVertices = 0;
            renderer.alignment = LineAlignment.View;
            renderer.textureMode = LineTextureMode.Stretch;

            if (lineMaterial != null)
            {
                renderer.sharedMaterial = lineMaterial;
            }
            else
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    renderer.sharedMaterial = new Material(shader);
                }
            }

            lines.Add(renderer);
        }

        public void ClearLines()
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(lines[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(lines[i].gameObject);
                    }
                }
            }
            lines.Clear();
        }

        private void OnDestroy()
        {
            ClearLines();
            if (lineRoot != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(lineRoot.gameObject);
                }
                else
                {
                    DestroyImmediate(lineRoot.gameObject);
                }
            }
        }
    }
}
