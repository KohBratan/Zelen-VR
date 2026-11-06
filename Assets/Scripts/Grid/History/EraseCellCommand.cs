using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Removes a cell from the grid and restores its previous contents on undo.
    /// </summary>
    public class EraseCellCommand : IGridCommand
    {
        private readonly GridManager manager;
        private readonly GridCoordinate coordinate;
        private readonly GridCell previous;
        private bool executed;

        public string Description { get { return "Erase " + coordinate; } }
        public bool IsExecuted { get { return executed; } }
        public int AffectedCellCount { get { return 1; } }
        public GridCoordinate Coordinate { get { return coordinate; } }
        public GridCell Previous { get { return previous; } }

        public EraseCellCommand(GridManager manager, GridCoordinate coordinate)
        {
            this.manager = manager;
            this.coordinate = coordinate;
            previous = manager != null ? manager.GetCell(coordinate) : GridCell.CreateEmpty(coordinate);
        }

        public EraseCellCommand(GridManager manager, GridCell previous)
        {
            this.manager = manager;
            this.previous = previous;
            coordinate = previous.Coordinate;
        }

        public void Execute()
        {
            if (manager == null)
            {
                return;
            }

            manager.RemoveCell(coordinate, true);
            executed = true;
        }

        public void Undo()
        {
            if (manager == null || previous.IsEmpty)
            {
                return;
            }

            manager.SetCell(previous, true);
            executed = false;
        }

        public override string ToString()
        {
            return "EraseCellCommand(" + coordinate + ")";
        }
    }
}
