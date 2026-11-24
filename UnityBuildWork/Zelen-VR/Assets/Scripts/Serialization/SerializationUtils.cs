using System;
using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Утилиты сериализации простых значений.</summary>
    public static class SerializationUtils
    {
        #region Vector
        public static string Vector3ToString(Vector3 v)
        {
            return string.Format("{0:F4};{1:F4};{2:F4}", v.x, v.y, v.z);
        }

        public static bool TryVector3FromString(string s, out Vector3 v)
        {
            v = Vector3.zero;
            if (string.IsNullOrEmpty(s)) return false;
            string[] parts = s.Split(';');
            if (parts.Length != 3) return false;
            if (!float.TryParse(parts[0], out float x)) return false;
            if (!float.TryParse(parts[1], out float y)) return false;
            if (!float.TryParse(parts[2], out float z)) return false;
            v = new Vector3(x, y, z);
            return true;
        }
        #endregion

        #region Quaternion
        public static string QuaternionToString(Quaternion q)
        {
            return string.Format("{0:F4};{1:F4};{2:F4};{3:F4}", q.x, q.y, q.z, q.w);
        }

        public static bool TryQuaternionFromString(string s, out Quaternion q)
        {
            q = Quaternion.identity;
            if (string.IsNullOrEmpty(s)) return false;
            string[] parts = s.Split(';');
            if (parts.Length != 4) return false;
            if (!float.TryParse(parts[0], out float x)) return false;
            if (!float.TryParse(parts[1], out float y)) return false;
            if (!float.TryParse(parts[2], out float z)) return false;
            if (!float.TryParse(parts[3], out float w)) return false;
            q = new Quaternion(x, y, z, w);
            return true;
        }
        #endregion

        #region Color
        public static string ColorToString(Color c)
        {
            return ColorUtility.ToHtmlStringRGBA(c);
        }

        public static bool TryColorFromString(string s, out Color c)
        {
            return ColorUtility.TryParseHtmlString(s.StartsWith("#") ? s : "#" + s, out c);
        }
        #endregion

        #region Bool
        public static string BoolToString(bool b) { return b ? "1" : "0"; }

        public static bool BoolFromString(string s)
        {
            return s == "1" || string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
