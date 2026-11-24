using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Позиционирование UI относительно игрока.</summary>
    public static class UIPositioning
    {
        #region Мировые координаты
        public static Vector3 WorldToScreen(Camera cam, Vector3 world)
        {
            if (cam == null) return Vector3.zero;
            return cam.WorldToScreenPoint(world);
        }

        public static Vector3 ScreenToWorld(Camera cam, Vector3 screen, float depth)
        {
            if (cam == null) return Vector3.zero;
            Vector3 p = screen; p.z = depth;
            return cam.ScreenToWorldPoint(p);
        }
        #endregion

        #region Привязка панели
        /// <summary>Разместить RectTransform над мировой точкой.</summary>
        public static void PlaceAboveWorldPoint(RectTransform panel, Camera cam, Vector3 world, Vector2 offset)
        {
            if (panel == null || cam == null) return;
            Vector3 sp = cam.WorldToScreenPoint(world);
            if (sp.z < 0f) { panel.gameObject.SetActive(false); return; }
            panel.gameObject.SetActive(true);
            panel.position = new Vector3(sp.x + offset.x, sp.y + offset.y, 0f);
        }
        #endregion

        #region Экран
        public static bool IsOnScreen(Camera cam, Vector3 world)
        {
            if (cam == null) return false;
            Vector3 sp = cam.WorldToScreenPoint(world);
            return sp.z > 0f && sp.x >= 0f && sp.x <= Screen.width && sp.y >= 0f && sp.y <= Screen.height;
        }
        #endregion
    }
}
