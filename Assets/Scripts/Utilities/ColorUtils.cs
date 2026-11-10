using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты работы с цветом.</summary>
    public static class ColorUtils
    {
        #region Модификация
        public static Color WithAlpha(Color c, float alpha)
        {
            return new Color(c.r, c.g, c.b, Mathf.Clamp01(alpha));
        }

        public static Color Multiply(Color c, float factor)
        {
            return new Color(
                Mathf.Clamp01(c.r * factor),
                Mathf.Clamp01(c.g * factor),
                Mathf.Clamp01(c.b * factor),
                c.a);
        }
        #endregion

        #region Градиенты
        public static Color Lerp(Color a, Color b, float t)
        {
            return Color.Lerp(a, b, Mathf.Clamp01(t));
        }
        #endregion

        #region Яркость
        public static float Luminance(Color c)
        {
            return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
        }

        public static Color Grayscale(Color c)
        {
            float l = Luminance(c);
            return new Color(l, l, l, c.a);
        }
        #endregion

        #region HEX
        public static string ToHex(Color c)
        {
            return "#" + ColorUtility.ToHtmlStringRGBA(c);
        }

        public static bool TryParseHex(string hex, out Color c)
        {
            return ColorUtility.TryParseHtmlString(hex, out c);
        }
        #endregion

        #region Палитра
        public static Color FromHSV(float h, float s, float v, float a = 1f)
        {
            return Color.HSVToRGB(Mathf.Repeat(h, 1f), Mathf.Clamp01(s), Mathf.Clamp01(v)) * new Color(1f, 1f, 1f, a);
        }
        #endregion
    }
}
