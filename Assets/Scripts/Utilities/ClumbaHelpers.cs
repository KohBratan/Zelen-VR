using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Хелперы для работы с клумбами.</summary>
    public static class ClumbaHelpers
    {
        #region Размеры клумбы
        public static Vector3 ComputeClumbaSize(int columns, int rows, float cellSize)
        {
            return new Vector3(columns * cellSize, 0.1f, rows * cellSize);
        }

        public static int CellCount(int columns, int rows)
        {
            return Mathf.Max(0, columns) * Mathf.Max(0, rows);
        }
        #endregion

        #region Позиции ячеек
        /// <summary>Мировая позиция ячейки (col,row) относительно центра клумбы.</summary>
        public static Vector3 CellToWorld(Vector3 origin, int col, int row, float cellSize, int columns, int rows)
        {
            float x = (col - (columns - 1) * 0.5f) * cellSize;
            float z = (row - (rows - 1) * 0.5f) * cellSize;
            return origin + new Vector3(x, 0f, z);
        }

        /// <summary>Индекс ячейки в одномерном массиве.</summary>
        public static int CellIndex(int col, int row, int columns)
        {
            return row * columns + col;
        }

        public static bool IsValidCell(int col, int row, int columns, int rows)
        {
            return col >= 0 && col < columns && row >= 0 && row < rows;
        }
        #endregion

        #region Заполненность
        public static float FillRatio(int occupied, int total)
        {
            if (total <= 0) return 0f;
            return Mathf.Clamp01((float)occupied / total);
        }

        public static string DescribeClumba(int columns, int rows, float cellSize)
        {
            return string.Format("Клумба {0}x{1}, шаг {2:F2} м, ячеек {3}",
                columns, rows, cellSize, CellCount(columns, rows));
        }
        #endregion
    }
}
