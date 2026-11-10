using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Undo/redo stack for grid edits. The component owns no data of its own;
    /// callers push already constructed commands and the history invokes them.
    /// </summary>
    [DisallowMultipleComponent]
    public class GridCommandHistory : MonoBehaviour
    {
        [Header("Limits")]
        [SerializeField, Min(1)] private int maxUndoSteps = 100;
        [SerializeField] private bool logCommands = false;

        private readonly List<IGridCommand> undoStack = new List<IGridCommand>();
        private readonly List<IGridCommand> redoStack = new List<IGridCommand>();

        public int UndoCount { get { return undoStack.Count; } }
        public int RedoCount { get { return redoStack.Count; } }
        public bool CanUndo { get { return undoStack.Count > 0; } }
        public bool CanRedo { get { return redoStack.Count > 0; } }
        public int MaxUndoSteps { get { return Mathf.Max(1, maxUndoSteps); } set { maxUndoSteps = Mathf.Max(1, value); TrimUndo(); } }

        public event Action<IGridCommand> CommandExecuted;
        public event Action<IGridCommand> CommandUndone;
        public event Action<IGridCommand> CommandRedone;
        public event Action HistoryCleared;

        /// <summary>Executes a new command and pushes it on the undo stack.</summary>
        public void Execute(IGridCommand command)
        {
            if (command == null)
            {
                return;
            }

            command.Execute();
            undoStack.Add(command);
            redoStack.Clear();
            TrimUndo();

            if (logCommands)
            {
                Debug.Log("Grid command executed: " + command.Description, this);
            }

            if (CommandExecuted != null)
            {
                CommandExecuted(command);
            }
        }

        /// <summary>Undoes the most recent command and moves it to the redo stack.</summary>
        public bool Undo()
        {
            if (undoStack.Count == 0)
            {
                return false;
            }

            IGridCommand command = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);
            command.Undo();
            redoStack.Add(command);

            if (logCommands)
            {
                Debug.Log("Grid command undone: " + command.Description, this);
            }

            if (CommandUndone != null)
            {
                CommandUndone(command);
            }
            return true;
        }

        /// <summary>Re-executes the most recently undone command.</summary>
        public bool Redo()
        {
            if (redoStack.Count == 0)
            {
                return false;
            }

            IGridCommand command = redoStack[redoStack.Count - 1];
            redoStack.RemoveAt(redoStack.Count - 1);
            command.Execute();
            undoStack.Add(command);

            if (logCommands)
            {
                Debug.Log("Grid command redone: " + command.Description, this);
            }

            if (CommandRedone != null)
            {
                CommandRedone(command);
            }
            return true;
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            if (HistoryCleared != null)
            {
                HistoryCleared();
            }
        }

        private void TrimUndo()
        {
            while (undoStack.Count > MaxUndoSteps)
            {
                undoStack.RemoveAt(0);
            }
        }

        public IReadOnlyList<IGridCommand> GetUndoCommands()
        {
            return undoStack;
        }

        public IReadOnlyList<IGridCommand> GetRedoCommands()
        {
            return redoStack;
        }
    }
}
