using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Paints a single cell and remembers the value that existed before the
    /// edit so the operation can be undone without consulting external state.
    /// </summary>
    public class PaintCellCommand : IGridCommand
    {
        private readonly GridManager manager;
        private readonly GridCoordinate coordinate;
        private readonly GridCell previous;
        private readonly GridCell next;
        private bool executed;

        public string Description { get { return "Paint " + coordinate; } }
        public bool IsExecuted { get { return executed; } }
        public int AffectedCellCount { get { return 1; } }
        public GridCoordinate Coordinate { get { return coordinate; } }
        public GridCell Previous { get { return previous; } }
        public GridCell Next { get { return next; } }

        public PaintCellCommand(GridManager manager, GridCell previous, GridCell next)
        {
            this.manager = manager;
            this.previous = previous;
            this.next = next;
            coordinate = next.Coordinate;
        }

        public PaintCellCommand(GridManager manager, GridCoordinate coordinate, int layer, int palette)
        {
            this.manager = manager;
            this.coordinate = coordinate;
            previous = manager != null ? manager.GetCell(coordinate) : GridCell.CreateEmpty(coordinate);
            next = GridCell.CreatePainted(coordinate, layer, palette);
        }

        public void Execute()
        {
            if (manager == null)
            {
                return;
            }

            if (next.IsEmpty)
            {
                manager.RemoveCell(coordinate, true);
            }
            else
            {
                manager.SetCell(next, true);
            }
            executed = true;
        }

        public void Undo()
        {
            if (manager == null)
            {
                return;
            }

            if (previous.IsEmpty)
            {
                manager.RemoveCell(coordinate, true);
            }
            else
            {
                manager.SetCell(previous, true);
            }
            executed = false;
        }

        public override string ToString()
        {
            return "PaintCellCommand(" + coordinate + ", layer=" + next.Layer + ")";
        }
    }
}
