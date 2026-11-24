using System;
using System.Collections.Generic;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты для перечислений.</summary>
    public static class EnumUtils
    {
        #region Значения
        public static T[] GetValues<T>() where T : struct, Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static string[] GetNames<T>() where T : struct, Enum
        {
            return Enum.GetNames(typeof(T));
        }
        #endregion

        #region Парсинг
        public static bool TryParse<T>(string value, out T result) where T : struct, Enum
        {
            return Enum.TryParse(value, true, out result);
        }

        public static T ParseOrDefault<T>(string value, T fallback) where T : struct, Enum
        {
            if (TryParse<T>(value, out T r)) return r;
            return fallback;
        }
        #endregion

        #region Соседи
        /// <summary>Циклический переход к следующему значению.</summary>
        public static T Next<T>(T current) where T : struct, Enum
        {
            T[] values = GetValues<T>();
            int idx = Array.IndexOf(values, current);
            return values[(idx + 1) % values.Length];
        }

        public static T Previous<T>(T current) where T : struct, Enum
        {
            T[] values = GetValues<T>();
            int idx = Array.IndexOf(values, current);
            return values[(idx - 1 + values.Length) % values.Length];
        }
        #endregion

        #region Флаги
        public static bool HasFlagInt(int flags, int flag)
        {
            return (flags & flag) == flag;
        }

        public static int SetFlag(int flags, int flag, bool on)
        {
            return on ? (flags | flag) : (flags & ~flag);
        }
        #endregion
    }
}
