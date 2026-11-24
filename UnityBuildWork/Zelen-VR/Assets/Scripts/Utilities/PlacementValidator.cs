using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Проверка корректности размещения объектов на участке.</summary>
    public static class PlacementValidator
    {
        #region Одиночная проверка
        public static bool HasOverlap(Bounds candidate, IList<Bounds> existing, float padding = 0f)
        {
            if (existing == null) return false;
            Bounds padded = new Bounds(candidate.center, candidate.size + Vector3.one * (padding * 2f));
            for (int i = 0; i < existing.Count; i++)
            {
                if (padded.Intersects(existing[i])) return true;
            }
            return false;
        }
        #endregion

        #region Поиск свободной позиции
        /// <summary>Найти свободную позицию в пределах области.</summary>
        public static bool TryFindFreeSpot(Bounds area, Vector2 objectSize, IList<Bounds> existing,
                                           int attempts, float padding, out Vector3 result)
        {
            result = area.center;
            for (int i = 0; i < attempts; i++)
            {
                float x = Random.Range(area.min.x, area.max.x);
                float z = Random.Range(area.min.z, area.max.z);
                Vector3 pos = new Vector3(x, area.center.y, z);
                Bounds cand = new Bounds(pos, new Vector3(objectSize.x, 0f, objectSize.y));
                if (!HasOverlap(cand, existing, padding))
                {
                    result = pos;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Сетка
        /// <summary>Проверить, что позиция выровнена по сетке.</summary>
        public static bool IsOnGrid(Vector3 pos, float cellSize, float tolerance = 0.01f)
        {
            if (cellSize <= 0f) return true;
            float rx = Mathf.Abs(pos.x / cellSize - Mathf.Round(pos.x / cellSize));
            float rz = Mathf.Abs(pos.z / cellSize - Mathf.Round(pos.z / cellSize));
            return rx * cellSize < tolerance && rz * cellSize < tolerance;
        }

        public static Vector3 SnapToGrid(Vector3 pos, float cellSize)
        {
            if (cellSize <= 0f) return pos;
            return new Vector3(
                Mathf.Round(pos.x / cellSize) * cellSize,
                pos.y,
                Mathf.Round(pos.z / cellSize) * cellSize);
        }
        #endregion
    }
}
