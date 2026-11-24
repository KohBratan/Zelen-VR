using System;
using System.Collections;
using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты для корутин.</summary>
    public static class CoroutineUtils
    {
        #region Задержки
        public static IEnumerator Delay(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }

        public static IEnumerator DelayRealtime(float seconds, Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);
            action?.Invoke();
        }
        #endregion

        #region Повтор
        public static IEnumerator Repeat(float interval, int count, Action action)
        {
            var wait = new WaitForSeconds(interval);
            for (int i = 0; i < count; i++)
            {
                action?.Invoke();
                yield return wait;
            }
        }

        public static IEnumerator RepeatUntil(Func<bool> condition, Action action)
        {
            while (condition != null && !condition())
            {
                action?.Invoke();
                yield return null;
            }
        }
        #endregion

        #region Ожидание
        public static IEnumerator WaitUntilTrue(Func<bool> condition)
        {
            while (condition != null && !condition()) yield return null;
        }

        public static IEnumerator WaitForFrames(int frames)
        {
            for (int i = 0; i < frames; i++) yield return null;
        }
        #endregion
    }
}
