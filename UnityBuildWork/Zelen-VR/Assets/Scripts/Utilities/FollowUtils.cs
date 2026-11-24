using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты позиционирования объектов за игроком.</summary>
    public static class FollowUtils
    {
        #region Позиции
        /// <summary>Позиция перед игроком на заданном расстоянии.</summary>
        public static Vector3 InFront(Transform player, float distance)
        {
            if (player == null) return Vector3.zero;
            Vector3 f = player.forward; f.y = 0f;
            if (f.sqrMagnitude < 1e-5f) f = Vector3.forward;
            return player.position + f.normalized * distance;
        }

        /// <summary>Позиция сбоку от игрока.</summary>
        public static Vector3 ToSide(Transform player, float distance, bool right = true)
        {
            if (player == null) return Vector3.zero;
            Vector3 r = player.right; r.y = 0f;
            if (r.sqrMagnitude < 1e-5f) r = Vector3.right;
            return player.position + r.normalized * distance * (right ? 1f : -1f);
        }
        #endregion

        #region Сглаживание
        public static Vector3 SmoothFollow(Vector3 current, Vector3 target, float speed, float dt)
        {
            float t = 1f - Mathf.Exp(-Mathf.Max(0f, speed) * dt);
            return Vector3.Lerp(current, target, Mathf.Clamp01(t));
        }

        public static Quaternion SmoothLookAt(Transform self, Vector3 target, float speed, float dt)
        {
            if (self == null) return Quaternion.identity;
            Vector3 dir = target - self.position; dir.y = 0f;
            if (dir.sqrMagnitude < 1e-5f) return self.rotation;
            Quaternion want = Quaternion.LookRotation(dir.normalized);
            return Quaternion.Slerp(self.rotation, want, Mathf.Clamp01(speed * dt));
        }
        #endregion
    }
}
