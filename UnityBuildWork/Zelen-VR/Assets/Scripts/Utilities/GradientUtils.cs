using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты для работы с градиентами.</summary>
    public static class GradientUtils
    {
        #region Создание
        public static Gradient TwoColor(Color a, Color b)
        {
            var g = new Gradient();
            g.SetKeys(
                new[]
                {
                    new GradientColorKey(a, 0f),
                    new GradientColorKey(b, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });
            return g;
        }

        public static Gradient ThreeColor(Color a, Color b, Color c)
        {
            var g = new Gradient();
            g.SetKeys(
                new[]
                {
                    new GradientColorKey(a, 0f),
                    new GradientColorKey(b, 0.5f),
                    new GradientColorKey(c, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.5f),
                    new GradientAlphaKey(1f, 1f)
                });
            return g;
        }
        #endregion

        #region Выборка
        public static Color Evaluate(Gradient g, float t)
        {
            if (g == null) return Color.white;
            return g.Evaluate(Mathf.Clamp01(t));
        }
        #endregion

        #region Модификация
        public static void SetAlpha(Gradient g, float alpha)
        {
            if (g == null) return;
            GradientAlphaKey[] keys = g.alphaKeys;
            for (int i = 0; i < keys.Length; i++) keys[i].alpha = alpha;
            g.alphaKeys = keys;
        }
        #endregion
    }
}
