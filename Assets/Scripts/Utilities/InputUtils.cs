using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты обработки ввода.</summary>
    public static class InputUtils
    {
        #region Клавиатура
        public static bool AnyKeyDown(params KeyCode[] keys)
        {
            if (keys == null) return false;
            for (int i = 0; i < keys.Length; i++)
                if (Input.GetKeyDown(keys[i])) return true;
            return false;
        }

        public static bool AllKeysHeld(params KeyCode[] keys)
        {
            if (keys == null) return false;
            for (int i = 0; i < keys.Length; i++)
                if (!Input.GetKey(keys[i])) return false;
            return keys.Length > 0;
        }
        #endregion

        #region Мышь
        public static bool MouseMoved(float threshold = 0.5f)
        {
            return Mathf.Abs(Input.GetAxis("Mouse X")) > threshold ||
                   Mathf.Abs(Input.GetAxis("Mouse Y")) > threshold;
        }
        #endregion

        #region Оси
        public static Vector2 ReadMoveAxis()
        {
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        public static Vector2 ReadMoveAxisRaw()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        public static Vector2 ClampAxis(Vector2 v)
        {
            return new Vector2(Mathf.Clamp(v.x, -1f, 1f), Mathf.Clamp(v.y, -1f, 1f));
        }
        #endregion

        #region Пороги
        public static bool ExceedsDeadZone(Vector2 v, float deadZone)
        {
            return v.sqrMagnitude > deadZone * deadZone;
        }
        #endregion
    }
}
