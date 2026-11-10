using System;
using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Геометрические вычисления для игровых объектов.</summary>
    public static class GeometryUtils
    {
        #region Площади и периметры
        public static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Cross(b - a, c - a).magnitude * 0.5f;
        }

        public static float PolygonPerimeter(Vector3[] points)
        {
            if (points == null || points.Length < 2) return 0f;
            float p = 0f;
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 a = points[i];
                Vector3 b = points[(i + 1) % points.Length];
                p += Vector3.Distance(a, b);
            }
            return p;
        }
        #endregion

        #region Пересечения
        /// <summary>Пересечение отрезка и плоскости.</summary>
        public static bool SegmentPlane(Vector3 p0, Vector3 p1, Vector3 planePoint, Vector3 planeNormal, out Vector3 hit)
        {
            hit = Vector3.zero;
            Vector3 dir = p1 - p0;
            float denom = Vector3.Dot(planeNormal, dir);
            if (Mathf.Abs(denom) < VectorMathUtils.Epsilon) return false;
            float t = Vector3.Dot(planeNormal, planePoint - p0) / denom;
            if (t < 0f || t > 1f) return false;
            hit = p0 + dir * t;
            return true;
        }

        /// <summary>Ближайшая точка на отрезке к заданной.</summary>
        public static Vector3 ClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float len = ab.sqrMagnitude;
            if (len < VectorMathUtils.Epsilon) return a;
            float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / len);
            return a + ab * t;
        }
        #endregion

        #region Окружности
        /// <summary>Точка на окружности в плоскости XZ.</summary>
        public static Vector3 PointOnCircle(Vector3 center, float radius, float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return center + new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
        }

        /// <summary>Равномерное размещение точек по окружности.</summary>
        public static Vector3[] PointsOnCircle(Vector3 center, float radius, int count)
        {
            if (count <= 0) return new Vector3[0];
            var result = new Vector3[count];
            float step = 360f / count;
            for (int i = 0; i < count; i++)
                result[i] = PointOnCircle(center, radius, i * step);
            return result;
        }
        #endregion

        #region Ограничивающие объёмы
        public static Bounds FromPoints(Vector3[] points)
        {
            if (points == null || points.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero);
            Bounds b = new Bounds(points[0], Vector3.zero);
            for (int i = 1; i < points.Length; i++) b.Encapsulate(points[i]);
            return b;
        }
        #endregion
    }
}
