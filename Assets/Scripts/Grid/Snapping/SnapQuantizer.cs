using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Pure quantization helpers shared by snap providers and editor tools.
    /// Every function is deterministic for a given step, which keeps layout
    /// results stable across runs.
    /// </summary>
    public static class SnapQuantizer
    {
        public const float Epsilon = 0.0001f;

        /// <summary>Rounds a scalar to the nearest multiple of step.</summary>
        public static float Quantize(float value, float step)
        {
            float s = Mathf.Max(Epsilon, step);
            return Mathf.Round(value / s) * s;
        }

        /// <summary>Floors a scalar to the nearest multiple of step.</summary>
        public static float QuantizeFloor(float value, float step)
        {
            float s = Mathf.Max(Epsilon, step);
            return Mathf.Floor(value / s) * s;
        }

        /// <summary>Ceils a scalar to the nearest multiple of step.</summary>
        public static float QuantizeCeil(float value, float step)
        {
            float s = Mathf.Max(Epsilon, step);
            return Mathf.Ceil(value / s) * s;
        }

        /// <summary>Quantizes each component of a vector by the same step.</summary>
        public static Vector3 Quantize(Vector3 value, float step)
        {
            return new Vector3(Quantize(value.x, step), Quantize(value.y, step), Quantize(value.z, step));
        }

        /// <summary>Quantizes only the axes allowed by the mask.</summary>
        public static Vector3 QuantizeMasked(Vector3 value, float step, SnapAxisMask mask)
        {
            Vector3 result = value;
            if ((mask & SnapAxisMask.X) != 0)
            {
                result.x = Quantize(value.x, step);
            }
            if ((mask & SnapAxisMask.Y) != 0)
            {
                result.y = Quantize(value.y, step);
            }
            if ((mask & SnapAxisMask.Z) != 0)
            {
                result.z = Quantize(value.z, step);
            }
            return result;
        }

        /// <summary>Quantizes a quaternion by snapping each Euler angle.</summary>
        public static Quaternion Quantize(Quaternion rotation, float step)
        {
            Vector3 euler = rotation.eulerAngles;
            euler.x = Quantize(euler.x, step);
            euler.y = Quantize(euler.y, step);
            euler.z = Quantize(euler.z, step);
            return Quaternion.Euler(euler);
        }

        /// <summary>True when the scalar already lies on the lattice within tolerance.</summary>
        public static bool IsOnLattice(float value, float step, float tolerance = 0.001f)
        {
            float quantized = Quantize(value, step);
            return Mathf.Abs(quantized - value) <= tolerance;
        }

        /// <summary>True when every component of the vector is on the lattice.</summary>
        public static bool IsOnLattice(Vector3 value, float step, float tolerance = 0.001f)
        {
            return IsOnLattice(value.x, step, tolerance)
                && IsOnLattice(value.y, step, tolerance)
                && IsOnLattice(value.z, step, tolerance);
        }

        /// <summary>Rounds a world position to the nearest grid cell centre.</summary>
        public static Vector3 SnapToGrid(Vector3 world, GridSettings settings)
        {
            if (settings == null)
            {
                return world;
            }
            return GridMath.SnapToCellCenter(world, settings);
        }

        /// <summary>Rounds a world position to the nearest grid corner.</summary>
        public static Vector3 SnapToCorner(Vector3 world, GridSettings settings)
        {
            if (settings == null)
            {
                return world;
            }
            return GridMath.SnapToCellCorner(world, settings);
        }

        /// <summary>Linear interpolation between two snapped values.</summary>
        public static float Lerp(float from, float to, float t, float step)
        {
            return Quantize(Mathf.Lerp(from, to, Mathf.Clamp01(t)), step);
        }

        public static Vector3 Lerp(Vector3 from, Vector3 to, float t, float step)
        {
            return Quantize(Vector3.Lerp(from, to, Mathf.Clamp01(t)), step);
        }
    }
}
