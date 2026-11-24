using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Вспомогательная логика захвата Grip.</summary>
    public static class GripCaptureHelper
    {
        #region Состояние
        /// <summary>Порог срабатывания захвата.</summary>
        public const float DefaultGripThreshold = 0.7f;

        public static bool IsGripPressed(float gripValue, float threshold = DefaultGripThreshold)
        {
            return gripValue >= threshold;
        }

        public static bool IsGripReleased(float gripValue, float releaseThreshold = 0.3f)
        {
            return gripValue <= releaseThreshold;
        }
        #endregion

        #region Захват
        /// <summary>Проверить, можно ли захватить объект в радиусе.</summary>
        public static bool CanCapture(Transform hand, Transform target, float maxDistance)
        {
            if (hand == null || target == null) return false;
            return Vector3.Distance(hand.position, target.position) <= maxDistance;
        }

        /// <summary>Точка захвата — ближайшая к руке часть коллайдера.</summary>
        public static Vector3 CapturePoint(Transform hand, Collider col)
        {
            if (col == null) return hand != null ? hand.position : Vector3.zero;
            if (hand == null) return col.bounds.center;
            return col.ClosestPoint(hand.position);
        }
        #endregion

        #region Удержание
        public static Vector3 HoldOffset(Transform hand, Vector3 objectPosition)
        {
            if (hand == null) return Vector3.zero;
            return objectPosition - hand.position;
        }

        public static Vector3 ApplyHold(Transform hand, Vector3 offset)
        {
            if (hand == null) return offset;
            return hand.position + offset;
        }
        #endregion
    }
}
