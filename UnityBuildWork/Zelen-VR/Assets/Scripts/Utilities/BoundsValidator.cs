using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Проверка границ размещаемых объектов.</summary>
    public static class BoundsValidator
    {
        #region Проверки
        /// <summary>Полностью ли объект внутри границ.</summary>
        public static bool IsInside(Bounds target, Bounds container)
        {
            return container.Contains(target.min) && container.Contains(target.max);
        }

        /// <summary>Пересекается ли объект с границами.</summary>
        public static bool Overlaps(Bounds target, Bounds container)
        {
            return target.Intersects(container);
        }

        /// <summary>Выходит ли объект за пределы по осям XZ.</summary>
        public static bool ExceedsXZ(Bounds target, Bounds container)
        {
            return target.min.x < container.min.x || target.max.x > container.max.x ||
                   target.min.z < container.min.z || target.max.z > container.max.z;
        }
        #endregion

        #region Коррекция
        /// <summary>Сместить границы внутрь контейнера.</summary>
        public static Bounds ClampInside(Bounds target, Bounds container)
        {
            Vector3 center = target.center;
            Vector3 ext = target.extents;
            Vector3 min = container.min + ext;
            Vector3 max = container.max - ext;
            center.x = Mathf.Clamp(center.x, min.x, Mathf.Max(min.x, max.x));
            center.z = Mathf.Clamp(center.z, min.z, Mathf.Max(min.z, max.z));
            return new Bounds(center, target.size);
        }
        #endregion

        #region Площадь
        public static float FlatArea(Bounds b)
        {
            return b.size.x * b.size.z;
        }

        public static float OverlapFlatArea(Bounds a, Bounds b)
        {
            float minX = Mathf.Max(a.min.x, b.min.x);
            float maxX = Mathf.Min(a.max.x, b.max.x);
            float minZ = Mathf.Max(a.min.z, b.min.z);
            float maxZ = Mathf.Min(a.max.z, b.max.z);
            if (maxX <= minX || maxZ <= minZ) return 0f;
            return (maxX - minX) * (maxZ - minZ);
        }
        #endregion
    }
}
