using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты для контейнеров растений.</summary>
    public static class PlantContainerUtils
    {
        #region Слоты
        /// <summary>Равномерно распределить слоты в контейнере.</summary>
        public static List<Vector3> DistributeSlots(Vector3 center, float radius, int count)
        {
            var list = new List<Vector3>(count);
            if (count <= 0) return list;
            float step = 360f / count;
            for (int i = 0; i < count; i++)
            {
                float rad = i * step * Mathf.Deg2Rad;
                list.Add(center + new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius));
            }
            return list;
        }

        /// <summary>Сетка слотов в прямоугольнике.</summary>
        public static List<Vector3> GridSlots(Vector3 center, int columns, int rows, float spacing)
        {
            var list = new List<Vector3>(columns * rows);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    float x = (c - (columns - 1) * 0.5f) * spacing;
                    float z = (r - (rows - 1) * 0.5f) * spacing;
                    list.Add(center + new Vector3(x, 0f, z));
                }
            }
            return list;
        }
        #endregion

        #region Занятость
        public static int OccupiedCount(bool[] slots)
        {
            if (slots == null) return 0;
            int n = 0;
            for (int i = 0; i < slots.Length; i++) if (slots[i]) n++;
            return n;
        }

        public static int FirstFreeSlot(bool[] slots)
        {
            if (slots == null) return -1;
            for (int i = 0; i < slots.Length; i++) if (!slots[i]) return i;
            return -1;
        }
        #endregion

        #region Проверка
        public static bool CanPlant(bool[] slots)
        {
            return FirstFreeSlot(slots) >= 0;
        }
        #endregion
    }
}
