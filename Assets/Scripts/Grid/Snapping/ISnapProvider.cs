using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>Result payload produced by a snap provider.</summary>
    public struct SnapResult
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public bool PositionSnapped;
        public bool RotationSnapped;
        public bool ScaleSnapped;

        public static SnapResult Unchanged(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return new SnapResult
            {
                Position = position,
                Rotation = rotation,
                Scale = scale,
                PositionSnapped = false,
                RotationSnapped = false,
                ScaleSnapped = false
            };
        }

        public override string ToString()
        {
            return "SnapResult(pos=" + Position + ", rot=" + Rotation.eulerAngles + ", scale=" + Scale + ")";
        }
    }

    /// <summary>
    /// Abstraction over anything that can quantize a transform to a lattice.
    /// Implementations must be stateless with respect to the values they
    /// return so callers can safely invoke them during layout passes.
    /// </summary>
    public interface ISnapProvider
    {
        bool Enabled { get; }

        SnapResult Snap(Vector3 position, Quaternion rotation, Vector3 scale);

        Vector3 SnapPosition(Vector3 position);

        Quaternion SnapRotation(Quaternion rotation);

        Vector3 SnapScale(Vector3 scale);
    }
}
