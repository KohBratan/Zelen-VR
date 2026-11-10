using System;
using System.Collections.Generic;

namespace ZelenVR.Utilities
{
    /// <summary>Расширения для списков.</summary>
    public static class ListExtensions
    {
        #region Безопасный доступ
        public static T GetSafe<T>(this IList<T> list, int index, T fallback = default(T))
        {
            if (list == null || index < 0 || index >= list.Count) return fallback;
            return list[index];
        }

        public static void Swap<T>(this IList<T> list, int a, int b)
        {
            if (list == null) return;
            if (a < 0 || b < 0 || a >= list.Count || b >= list.Count) return;
            T tmp = list[a]; list[a] = list[b]; list[b] = tmp;
        }
        #endregion

        #region Перемешивание
        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            if (list == null || rng == null) return;
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                Swap(list, i, j);
            }
        }
        #endregion

        #region Удаление
        public static int RemoveAllFrom<T>(this IList<T> list, ICollection<T> toRemove)
        {
            if (list == null || toRemove == null) return 0;
            int removed = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (toRemove.Contains(list[i])) { list.RemoveAt(i); removed++; }
            }
            return removed;
        }
        #endregion

        #region Поиск
        public static int IndexOfMatch<T>(this IList<T> list, Predicate<T> predicate)
        {
            if (list == null || predicate == null) return -1;
            for (int i = 0; i < list.Count; i++)
                if (predicate(list[i])) return i;
            return -1;
        }
        #endregion
    }
}
