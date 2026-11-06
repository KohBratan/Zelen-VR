using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Central runtime owner of a single grid instance. The manager stores the
    /// sparse set of cells that have been touched by editing operations and
    /// raises events whenever the contents change. Rendering, input and history
    /// systems talk to this component instead of keeping their own cell maps.
    /// </summary>
    [DisallowMultipleComponent]
    public class GridManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GridSettings settings;

        [Header("Lifecycle")]
        [SerializeField] private bool initializeOnAwake = true;

        private readonly Dictionary<GridCoordinate, GridCell> cells =
            new Dictionary<GridCoordinate, GridCell>();

        private bool initialized;

        public static GridManager Instance { get; private set; }

        public GridSettings Settings { get { return settings; } }
        public bool IsInitialized { get { return initialized; } }
        public int CellCount { get { return cells.Count; } }

        public event Action<GridSettings> GridInitialized;
        public event Action GridCleared;
        public event Action<GridCoordinate, GridCell> CellChanged;
        public event Action<GridBounds> BoundsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple GridManager instances detected. Keeping the first one.", this);
                enabled = false;
                return;
            }

            Instance = this;
            if (initializeOnAwake && settings != null)
            {
                Initialize(settings);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Binds the manager to a settings asset and prepares the cell storage.
        /// Calling this again with a different asset clears the previous data.
        /// </summary>
        public void Initialize(GridSettings newSettings)
        {
            if (newSettings == null)
            {
                Debug.LogError("GridManager.Initialize received a null settings asset.", this);
                return;
            }

            settings = newSettings;
            cells.Clear();
            initialized = true;

            if (GridInitialized != null)
            {
                GridInitialized(settings);
            }

            RaiseBoundsChanged();
        }

        /// <summary>Clears all painted cells but keeps the current settings.</summary>
        public void Clear()
        {
            cells.Clear();
            if (GridCleared != null)
            {
                GridCleared();
            }
            RaiseBoundsChanged();
        }

        public bool Contains(GridCoordinate coordinate)
        {
            return settings != null && settings.Contains(coordinate);
        }

        public bool HasCell(GridCoordinate coordinate)
        {
            return cells.ContainsKey(coordinate);
        }

        public GridCell GetCell(GridCoordinate coordinate)
        {
            GridCell cell;
            if (cells.TryGetValue(coordinate, out cell))
            {
                return cell;
            }
            return GridCell.CreateEmpty(coordinate);
        }

        public bool TryGetCell(GridCoordinate coordinate, out GridCell cell)
        {
            return cells.TryGetValue(coordinate, out cell);
        }

        /// <summary>
        /// Writes a cell into the sparse map. Empty cells are removed so the map
        /// only contains meaningful entries.
        /// </summary>
        public bool SetCell(GridCell cell, bool silent = false)
        {
            if (settings == null)
            {
                return false;
            }

            if (!settings.Contains(cell.Coordinate))
            {
                return false;
            }

            GridCell previous;
            bool hadPrevious = cells.TryGetValue(cell.Coordinate, out previous);

            if (cell.IsEmpty)
            {
                if (!hadPrevious)
                {
                    return false;
                }
                cells.Remove(cell.Coordinate);
            }
            else
            {
                if (hadPrevious && previous.Equals(cell))
                {
                    return false;
                }
                cells[cell.Coordinate] = cell;
            }

            if (!silent && CellChanged != null)
            {
                CellChanged(cell.Coordinate, cell);
            }
            RaiseBoundsChanged();
            return true;
        }

        public bool RemoveCell(GridCoordinate coordinate, bool silent = false)
        {
            if (!cells.Remove(coordinate))
            {
                return false;
            }

            if (!silent && CellChanged != null)
            {
                CellChanged(coordinate, GridCell.CreateEmpty(coordinate));
            }
            RaiseBoundsChanged();
            return true;
        }

        public IEnumerable<KeyValuePair<GridCoordinate, GridCell>> EnumerateCells()
        {
            return cells;
        }

        public List<GridCell> GetCellsInBounds(GridBounds bounds)
        {
            List<GridCell> result = new List<GridCell>();
            foreach (KeyValuePair<GridCoordinate, GridCell> pair in cells)
            {
                if (bounds.Contains(pair.Key))
                {
                    result.Add(pair.Value);
                }
            }
            return result;
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

        private void RaiseBoundsChanged()
        {
            if (BoundsChanged != null)
            {
                BoundsChanged(CalculateBounds());
            }
        }
    }
}
