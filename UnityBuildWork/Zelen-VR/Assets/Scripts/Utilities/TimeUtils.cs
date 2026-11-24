using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты работы со временем.</summary>
    public static class TimeUtils
    {
        #region Форматирование
        public static string FormatClock(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            int total = Mathf.FloorToInt(seconds);
            int h = total / 3600;
            int m = (total % 3600) / 60;
            int s = total % 60;
            if (h > 0) return string.Format("{0}:{1:00}:{2:00}", h, m, s);
            return string.Format("{0:00}:{1:00}", m, s);
        }
        #endregion

        #region Масштаб
        public static float ScaledDelta(float scale)
        {
            return Time.deltaTime * scale;
        }

        public static float UnscaledDelta()
        {
            return Time.unscaledDeltaTime;
        }
        #endregion

        #region Интерполяция
        public static float SmoothDamp01(float current, float target, ref float velocity, float smoothTime)
        {
            return Mathf.SmoothDamp(current, target, ref velocity, Mathf.Max(0.0001f, smoothTime));
        }
        #endregion

        #region Утилиты
        public static bool HasElapsed(float startTime, float duration)
        {
            return Time.time - startTime >= duration;
        }

        public static float TimeSince(float startTime)
        {
            return Mathf.Max(0f, Time.time - startTime);
        }
        #endregion
    }
}
