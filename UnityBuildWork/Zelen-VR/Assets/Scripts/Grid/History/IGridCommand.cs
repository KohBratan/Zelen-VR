using System;

namespace ZelenVR.Grid
{
    /// <summary>
    /// A single reversible edit performed on grid data. Commands are stored by
    /// GridCommandHistory and must be safe to execute more than once after an
    /// undo/redo cycle.
    /// </summary>
    public interface IGridCommand
    {
        string Description { get; }
        bool IsExecuted { get; }
        int AffectedCellCount { get; }

        void Execute();
        void Undo();
    }
}
