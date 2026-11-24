using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Обёртка над JsonUtility.</summary>
    public static class JsonHelper
    {
        #region Сериализация
        public static string ToJson<T>(T data, bool pretty = false)
        {
            if (data == null) return string.Empty;
            return JsonUtility.ToJson(data, pretty);
        }

        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default(T);
            return JsonUtility.FromJson<T>(json);
        }

        public static bool TryFromJson<T>(string json, out T result)
        {
            result = default(T);
            if (string.IsNullOrEmpty(json)) return false;
            try
            {
                result = JsonUtility.FromJson<T>(json);
                return result != null;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        #endregion

        #region Массивы
        /// <summary>Сериализовать массив (через обёртку).</summary>
        public static string ArrayToJson<T>(T[] items, bool pretty = false)
        {
            var wrapper = new ArrayWrapper<T> { items = items ?? new T[0] };
            return JsonUtility.ToJson(wrapper, pretty);
        }

        public static T[] ArrayFromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return new T[0];
            var wrapper = JsonUtility.FromJson<ArrayWrapper<T>>(json);
            return wrapper != null && wrapper.items != null ? wrapper.items : new T[0];
        }
        #endregion

        [System.Serializable]
        private class ArrayWrapper<T>
        {
            public T[] items;
        }
    }
}
