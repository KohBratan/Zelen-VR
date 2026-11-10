using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Хелперы сохранения через PlayerPrefs.</summary>
    public static class SaveDataUtils
    {
        #region Строки
        public static void SaveString(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            PlayerPrefs.SetString(key, value ?? string.Empty);
        }

        public static string LoadString(string key, string fallback = "")
        {
            if (string.IsNullOrEmpty(key)) return fallback;
            return PlayerPrefs.GetString(key, fallback);
        }
        #endregion

        #region Числа
        public static void SaveInt(string key, int value)
        {
            if (string.IsNullOrEmpty(key)) return;
            PlayerPrefs.SetInt(key, value);
        }

        public static int LoadInt(string key, int fallback = 0)
        {
            if (string.IsNullOrEmpty(key)) return fallback;
            return PlayerPrefs.GetInt(key, fallback);
        }

        public static void SaveFloat(string key, float value)
        {
            if (string.IsNullOrEmpty(key)) return;
            PlayerPrefs.SetFloat(key, value);
        }

        public static float LoadFloat(string key, float fallback = 0f)
        {
            if (string.IsNullOrEmpty(key)) return fallback;
            return PlayerPrefs.GetFloat(key, fallback);
        }
        #endregion

        #region Наличие
        public static bool HasKey(string key)
        {
            return !string.IsNullOrEmpty(key) && PlayerPrefs.HasKey(key);
        }

        public static void DeleteKey(string key)
        {
            if (!string.IsNullOrEmpty(key)) PlayerPrefs.DeleteKey(key);
        }
        #endregion

        #region Сохранение
        public static void Flush()
        {
            PlayerPrefs.Save();
        }
        #endregion
    }
}
