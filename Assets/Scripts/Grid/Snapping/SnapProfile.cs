// ---------------------------------------------------------------------------
// Zelen-VR
// File: SnapProfile.cs
// Profil privyazki.
// ---------------------------------------------------------------------------
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>Which world axes participate in snapping.</summary>
    [System.Flags]
    public enum SnapAxisMask
    {
        None = 0,
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        XY = X | Y,
        XZ = X | Z,
        YZ = Y | Z,
        XYZ = X | Y | Z
    }

    /// <summary>
    /// ScriptableObject describing how transforms are quantized. A profile is
    /// shared between many objects; per-object offsets live on SnapAnchor so a
    /// single asset can serve an entire scene.
    /// </summary>
    [CreateAssetMenu(fileName = "SnapProfile", menuName = "ZelenVR/Grid/Snap Profile", order = 1)]
    public class SnapProfile : ScriptableObject
    {
        [Header("Position")]
        [SerializeField, Min(0.0001f)] private float positionStep = 0.25f;
        [SerializeField] private SnapAxisMask positionAxes = SnapAxisMask.XYZ;
        [SerializeField] private Vector3 positionOffset = Vector3.zero;
        [SerializeField] private bool snapPosition = true;

        [Header("Rotation")]
        [SerializeField, Min(0.01f)] private float rotationStep = 15f;
        [SerializeField] private bool snapRotation = true;

        [Header("Scale")]
        [SerializeField, Min(0.0001f)] private float scaleStep = 0.25f;
        [SerializeField] private bool snapScale = false;

        [Header("Grid")]
        [SerializeField] private GridSettings gridSettings;

        public float PositionStep { get { return Mathf.Max(0.0001f, positionStep); } set { positionStep = Mathf.Max(0.0001f, value); } }
        public SnapAxisMask PositionAxes { get { return positionAxes; } set { positionAxes = value; } }
        public Vector3 PositionOffset { get { return positionOffset; } set { positionOffset = value; } }
        public bool SnapPosition { get { return snapPosition; } set { snapPosition = value; } }
        public float RotationStep { get { return Mathf.Max(0.01f, rotationStep); } set { rotationStep = Mathf.Max(0.01f, value); } }
        public bool SnapRotation { get { return snapRotation; } set { snapRotation = value; } }
        public float ScaleStep { get { return Mathf.Max(0.0001f, scaleStep); } set { scaleStep = Mathf.Max(0.0001f, value); } }
        public bool SnapScale { get { return snapScale; } set { snapScale = value; } }
        public GridSettings GridSettings { get { return gridSettings; } set { gridSettings = value; } }

        public bool IsAxisEnabled(SnapAxisMask axis)
        {
            return (positionAxes & axis) == axis;
        }

        /// <summary>Effective step, preferring the linked grid's cell size when present.</summary>
        public float ResolvePositionStep()
        {
            if (gridSettings != null)
            {
                return gridSettings.CellSize;
            }
            return PositionStep;
        }

        private void OnValidate()
        {
            positionStep = Mathf.Max(0.0001f, positionStep);
            rotationStep = Mathf.Max(0.01f, rotationStep);
            scaleStep = Mathf.Max(0.0001f, scaleStep);
        }
    }
}
