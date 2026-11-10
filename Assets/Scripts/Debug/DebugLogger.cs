using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Простой отладочный логгер с категориями.</summary>
    public static class DebugLogger
    {
        #region Состояние
        private static readonly HashSet<string> _enabled = new HashSet<string>();
        private static bool _defaultEnabled = true;
        #endregion

        #region Настройка
        public static void SetDefaultEnabled(bool enabled) { _defaultEnabled = enabled; }

        public static void Enable(string category)
        {
            if (!string.IsNullOrEmpty(category)) _enabled.Add(category);
        }

        public static void Disable(string category)
        {
            if (!string.IsNullOrEmpty(category)) _enabled.Remove(category);
        }

        public static bool IsEnabled(string category)
        {
            if (string.IsNullOrEmpty(category)) return _defaultEnabled;
            return _enabled.Contains(category) || _defaultEnabled;
        }
        #endregion

        #region Вывод
        public static void Log(string category, string message)
        {
            if (!IsEnabled(category)) return;
            Debug.Log(Format(category, message));
        }

        public static void Warning(string category, string message)
        {
            if (!IsEnabled(category)) return;
            Debug.LogWarning(Format(category, message));
        }

        public static void Error(string category, string message)
        {
            Debug.LogError(Format(category, message));
        }

        public static void Exception(string category, Exception ex)
        {
            if (ex == null) { Error(category, "null exception"); return; }
            Debug.LogError(Format(category, ex.ToString()));
        }
        #endregion

        #region Формат
        private static string Format(string category, string message)
        {
            string cat = string.IsNullOrEmpty(category) ? "General" : category;
            return "[" + cat + "] " + message;
        }
        #endregion
    }
}
