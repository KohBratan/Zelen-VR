using System;
using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Вспомогательные методы для векторной математики.</summary>
    public static class VectorMathUtils
    {
        #region Константы
        public const float Epsilon = 1e-5f;
        #endregion

        #region Скалярные операции
        /// <summary>Безопасная длина вектора.</summary>
        public static float SafeMagnitude(Vector3 v)
        {
            return Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        /// <summary>Нормализация с защитой от деления на ноль.</summary>
        public static Vector3 SafeNormalize(Vector3 v)
        {
            float m = SafeMagnitude(v);
            return m > Epsilon ? v / m : Vector3.zero;
        }

        /// <summary>Ограничение длины вектора.</summary>
        public static Vector3 ClampMagnitude(Vector3 v, float max)
        {
            float m = SafeMagnitude(v);
            if (m > max && m > Epsilon) return v * (max / m);
            return v;
        }

        /// <summary>Проекция a на b.</summary>
        public static Vector3 Project(Vector3 a, Vector3 b)
        {
            float d = Vector3.Dot(b, b);
            if (d < Epsilon) return Vector3.zero;
            return b * (Vector3.Dot(a, b) / d);
        }

        /// <summary>Отражение относительно нормали.</summary>
        public static Vector3 Reflect(Vector3 v, Vector3 normal)
        {
            return v - 2f * Vector3.Dot(v, normal) * normal;
        }
        #endregion

        #region Угловые операции
        public static float LerpAngle(float from, float to, float t)
        {
            float delta = Mathf.DeltaAngle(from, to);
            return from + delta * Mathf.Clamp01(t);
        }

        public static float WrapAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            return angle;
        }
        #endregion

        #region Плоские проекции
        public static Vector3 FlattenXZ(Vector3 v) { return new Vector3(v.x, 0f, v.z); }
        public static Vector3 FlattenXY(Vector3 v) { return new Vector3(v.x, v.y, 0f); }

        public static Vector3 RotateY(Vector3 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float c = Mathf.Cos(rad);
            float s = Mathf.Sin(rad);
            return new Vector3(v.x * c + v.z * s, v.y, -v.x * s + v.z * c);
        }
        #endregion

        #region Интерполяция
        public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t)
        {
            return a + (b - a) * t;
        }

        public static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0 + Epsilon));
            return t * t * (3f - 2f * t);
        }
        #endregion
    }
}
