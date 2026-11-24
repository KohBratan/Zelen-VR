using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты для безопасной работы с Transform.</summary>
    public static class TransformUtils
    {
        #region Позиционирование
        public static void SetLocalPositionX(Transform t, float x)
        {
            if (t == null) return;
            Vector3 p = t.localPosition; p.x = x; t.localPosition = p;
        }

        public static void SetLocalPositionY(Transform t, float y)
        {
            if (t == null) return;
            Vector3 p = t.localPosition; p.y = y; t.localPosition = p;
        }

        public static void SetLocalPositionZ(Transform t, float z)
        {
            if (t == null) return;
            Vector3 p = t.localPosition; p.z = z; t.localPosition = p;
        }
        #endregion

        #region Иерархия
        /// <summary>Найти дочерний объект по имени рекурсивно.</summary>
        public static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent == null) return null;
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>Удалить всех детей.</summary>
        public static void DestroyChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(parent.GetChild(i).gameObject);
            }
        }
        #endregion

        #region Направления
        public static Vector3 FlatForward(Transform t)
        {
            if (t == null) return Vector3.forward;
            Vector3 f = t.forward; f.y = 0f;
            return f.sqrMagnitude > 1e-5f ? f.normalized : Vector3.forward;
        }

        public static Vector3 FlatRight(Transform t)
        {
            if (t == null) return Vector3.right;
            Vector3 r = t.right; r.y = 0f;
            return r.sqrMagnitude > 1e-5f ? r.normalized : Vector3.right;
        }
        #endregion

        #region Копирование
        public static void CopyLocal(Transform from, Transform to)
        {
            if (from == null || to == null) return;
            to.localPosition = from.localPosition;
            to.localRotation = from.localRotation;
            to.localScale = from.localScale;
        }
        #endregion
    }
}
