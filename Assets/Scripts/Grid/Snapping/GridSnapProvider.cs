// ---------------------------------------------------------------------------
// Zelen-VR
// File: GridSnapProvider.cs
// Provayder privyazki.
// ---------------------------------------------------------------------------
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Snap provider backed by a SnapProfile asset. The component can be used
    /// directly from the inspector or instantiated at runtime and fed a profile
    /// by other systems.
    /// </summary>
    [DisallowMultipleComponent]
    public class GridSnapProvider : MonoBehaviour, ISnapProvider
    {
        [SerializeField] private SnapProfile profile;
        [SerializeField] private bool enabledSnapping = true;

        public SnapProfile Profile
        {
            get { return profile; }
            set { profile = value; }
        }

        public bool Enabled
        {
            get { return enabledSnapping && profile != null; }
            set { enabledSnapping = value; }
        }

        public SnapResult Snap(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (!Enabled)
            {
                return SnapResult.Unchanged(position, rotation, scale);
            }

            SnapResult result = new SnapResult
            {
                Position = position,
                Rotation = rotation,
                Scale = scale
            };

            if (profile.SnapPosition)
            {
                result.Position = SnapPosition(position);
                result.PositionSnapped = true;
            }

            if (profile.SnapRotation)
            {
                result.Rotation = SnapRotation(rotation);
                result.RotationSnapped = true;
            }

            if (profile.SnapScale)
            {
                result.Scale = SnapScale(scale);
                result.ScaleSnapped = true;
            }

            return result;
        }

        public Vector3 SnapPosition(Vector3 position)
        {
            if (!Enabled || !profile.SnapPosition)
            {
                return position;
            }

            Vector3 offset = profile.PositionOffset;
            Vector3 local = position - offset;

            Vector3 snapped;
            if (profile.GridSettings != null)
            {
                snapped = GridMath.SnapToCellCenter(local, profile.GridSettings);
            }
            else
            {
                snapped = SnapQuantizer.QuantizeMasked(local, profile.PositionStep, profile.PositionAxes);
            }

            return snapped + offset;
        }

        public Quaternion SnapRotation(Quaternion rotation)
        {
            if (!Enabled || !profile.SnapRotation)
            {
                return rotation;
            }
            return SnapQuantizer.Quantize(rotation, profile.RotationStep);
        }

        public Vector3 SnapScale(Vector3 scale)
        {
            if (!Enabled || !profile.SnapScale)
            {
                return scale;
            }
            return SnapQuantizer.Quantize(scale, profile.ScaleStep);
        }

        /// <summary>Applies the snapped transform directly to a target transform.</summary>
        public void ApplyTo(Transform target)
        {
            if (target == null)
            {
                return;
            }
            SnapResult result = Snap(target.position, target.rotation, target.localScale);
            target.position = result.Position;
            target.rotation = result.Rotation;
            target.localScale = result.Scale;
        }
    }
}
