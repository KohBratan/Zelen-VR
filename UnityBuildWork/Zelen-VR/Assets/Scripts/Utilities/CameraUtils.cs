using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Вспомогательные операции с камерой.</summary>
    public static class CameraUtils
    {
        #region Луч
        public static bool ScreenRay(Camera cam, Vector2 screen, out Ray ray)
        {
            ray = default(Ray);
            if (cam == null) return false;
            ray = cam.ScreenPointToRay(screen);
            return true;
        }

        /// <summary>Пересечение луча с плоскостью Y=planeY.</summary>
        public static bool RaycastGround(Camera cam, Vector2 screen, float planeY, out Vector3 hit)
        {
            hit = Vector3.zero;
            if (!ScreenRay(cam, screen, out Ray ray)) return false;
            Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
            if (plane.Raycast(ray, out float dist))
            {
                hit = ray.GetPoint(dist);
                return true;
            }
            return false;
        }
        #endregion

        #region Область видимости
        public static bool IsInViewport(Camera cam, Vector3 world, float margin = 0f)
        {
            if (cam == null) return false;
            Vector3 vp = cam.WorldToViewportPoint(world);
            return vp.z > 0f &&
                   vp.x >= -margin && vp.x <= 1f + margin &&
                   vp.y >= -margin && vp.y <= 1f + margin;
        }
        #endregion

        #region Масштабирование
        public static float DistanceTo(Camera cam, Vector3 world)
        {
            if (cam == null) return 0f;
            return Vector3.Distance(cam.transform.position, world);
        }
        #endregion
    }
}
