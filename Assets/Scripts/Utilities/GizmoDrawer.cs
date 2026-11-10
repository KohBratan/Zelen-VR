using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты рисования гизмо в редакторе и рантайме.</summary>
    public static class GizmoDrawer
    {
        #region Сферы
        public static void DrawWireSphere(Vector3 center, float radius, Color color)
        {
            Color prev = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(center, radius);
            Gizmos.color = prev;
        }

        public static void DrawSolidSphere(Vector3 center, float radius, Color color)
        {
            Color prev = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawSphere(center, radius);
            Gizmos.color = prev;
        }
        #endregion

        #region Кубы
        public static void DrawWireCube(Vector3 center, Vector3 size, Color color)
        {
            Color prev = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawWireCube(center, size);
            Gizmos.color = prev;
        }

        public static void DrawBounds(Bounds bounds, Color color)
        {
            DrawWireCube(bounds.center, bounds.size, color);
        }
        #endregion

        #region Линии
        public static void DrawLine(Vector3 from, Vector3 to, Color color)
        {
            Color prev = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawLine(from, to);
            Gizmos.color = prev;
        }

        public static void DrawRay(Vector3 origin, Vector3 direction, Color color)
        {
            Color prev = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawRay(origin, direction);
            Gizmos.color = prev;
        }
        #endregion

        #region Сетка
        public static void DrawGrid(Vector3 center, int halfSize, float cell, Color color)
        {
            Color prev = Gizmos.color;
            Gizmos.color = color;
            float extent = halfSize * cell;
            for (int i = -halfSize; i <= halfSize; i++)
            {
                float off = i * cell;
                Gizmos.DrawLine(center + new Vector3(off, 0f, -extent), center + new Vector3(off, 0f, extent));
                Gizmos.DrawLine(center + new Vector3(-extent, 0f, off), center + new Vector3(extent, 0f, off));
            }
            Gizmos.color = prev;
        }
        #endregion
    }
}
