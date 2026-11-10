using System;
using System.Text;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты форматирования строк.</summary>
    public static class StringFormatting
    {
        #region Числа
        public static string Percent(float value, int digits = 0)
        {
            return (value * 100f).ToString("F" + digits) + "%";
        }

        public static string CompactNumber(long value)
        {
            if (value >= 1000000) return (value / 1000000f).ToString("F1") + "M";
            if (value >= 1000) return (value / 1000f).ToString("F1") + "K";
            return value.ToString();
        }
        #endregion

        #region Время
        public static string Time(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            int total = (int)seconds;
            int m = total / 60;
            int s = total % 60;
            return string.Format("{0:00}:{1:00}", m, s);
        }
        #endregion

        #region Усечение
        public static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text;
            return text.Substring(0, Math.Max(0, maxLength - 3)) + "...";
        }

        public static string PadCenter(string text, int width, char fill = ' ')
        {
            if (text == null) text = string.Empty;
            if (text.Length >= width) return text;
            int total = width - text.Length;
            int left = total / 2;
            int right = total - left;
            return new string(fill, left) + text + new string(fill, right);
        }
        #endregion

        #region Сборка
        public static string Join<T>(string sep, System.Collections.Generic.IEnumerable<T> items)
        {
            if (items == null) return string.Empty;
            var sb = new StringBuilder();
            bool first = true;
            foreach (T item in items)
            {
                if (!first) sb.Append(sep);
                sb.Append(item);
                first = false;
            }
            return sb.ToString();
        }
        #endregion
    }
}
