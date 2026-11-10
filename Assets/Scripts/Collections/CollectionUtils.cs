using System.Collections.Generic;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты для работы с коллекциями.</summary>
    public static class CollectionUtils
    {
        #region Проверки
        public static bool IsNullOrEmpty<T>(ICollection<T> col)
        {
            return col == null || col.Count == 0;
        }

        public static int SafeCount<T>(ICollection<T> col)
        {
            return col == null ? 0 : col.Count;
        }
        #endregion

        #region Добавление
        public static void AddIfMissing<T>(IList<T> list, T item)
        {
            if (list == null) return;
            if (!list.Contains(item)) list.Add(item);
        }

        public static void AddRangeUnique<T>(IList<T> list, IEnumerable<T> items)
        {
            if (list == null || items == null) return;
            foreach (T item in items) AddIfMissing(list, item);
        }
        #endregion

        #region Разбиение
        public static List<List<T>> Chunk<T>(IList<T> source, int size)
        {
            var result = new List<List<T>>();
            if (source == null || size <= 0) return result;
            for (int i = 0; i < source.Count; i += size)
            {
                var chunk = new List<T>();
                for (int j = i; j < i + size && j < source.Count; j++)
                    chunk.Add(source[j]);
                result.Add(chunk);
            }
            return result;
        }
        #endregion

        #region Словари
        public static V GetOrDefault<K, V>(IDictionary<K, V> dict, K key, V fallback = default(V))
        {
            if (dict != null && dict.TryGetValue(key, out V v)) return v;
            return fallback;
        }

        public static void Increment<K>(IDictionary<K, int> dict, K key, int amount = 1)
        {
            if (dict == null) return;
            dict.TryGetValue(key, out int cur);
            dict[key] = cur + amount;
        }
        #endregion
    }
}
