using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Набор стандартных категорий логирования.</summary>
    public static class LogCategories
    {
        public const string General = "General";
        public const string Grid = "Grid";
        public const string Clumba = "Clumba";
        public const string UI = "UI";
        public const string Input = "Input";
        public const string Inventory = "Inventory";
        public const string Serialization = "Serialization";
        public const string Performance = "Performance";
        public const string StateMachine = "StateMachine";
        public const string Events = "Events";

        #region Регистрация
        /// <summary>Включить стандартный набор категорий.</summary>
        public static void EnableAll()
        {
            DebugLogger.Enable(General);
            DebugLogger.Enable(Grid);
            DebugLogger.Enable(Clumba);
            DebugLogger.Enable(UI);
            DebugLogger.Enable(Input);
            DebugLogger.Enable(Inventory);
            DebugLogger.Enable(Serialization);
            DebugLogger.Enable(Performance);
            DebugLogger.Enable(StateMachine);
            DebugLogger.Enable(Events);
        }

        public static void DisableAll()
        {
            DebugLogger.Disable(General);
            DebugLogger.Disable(Grid);
            DebugLogger.Disable(Clumba);
            DebugLogger.Disable(UI);
            DebugLogger.Disable(Input);
            DebugLogger.Disable(Inventory);
            DebugLogger.Disable(Serialization);
            DebugLogger.Disable(Performance);
            DebugLogger.Disable(StateMachine);
            DebugLogger.Disable(Events);
        }
        #endregion
    }
}
