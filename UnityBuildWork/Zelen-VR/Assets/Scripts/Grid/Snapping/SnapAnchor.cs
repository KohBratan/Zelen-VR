using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Optional per-object anchor that snaps its own transform on enable and
    /// whenever SnapNow is called. Useful for props that should stay aligned to
    /// the drawing grid without a global layout manager.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public class SnapAnchor : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private SnapProfile profile;
        [SerializeField] private bool useProviderOnSameObject = true;

        [Header("Behaviour")]
        [SerializeField] private bool snapOnEnable = true;
        [SerializeField] private bool snapOnDisable = false;
        [SerializeField] private bool snapContinuously = false;
        [SerializeField] private Vector3 localOffset = Vector3.zero;

        private GridSnapProvider cachedProvider;
        private ISnapProvider provider;

        public SnapProfile Profile
        {
            get { return profile; }
            set { profile = value; }
        }

        private void Awake()
        {
            if (useProviderOnSameObject)
            {
                cachedProvider = GetComponent<GridSnapProvider>();
            }
        }

        private void OnEnable()
        {
            if (snapOnEnable)
            {
                SnapNow();
            }
        }

        private void OnDisable()
        {
            if (snapOnDisable)
            {
                SnapNow();
            }
        }

        private void LateUpdate()
        {
            if (snapContinuously)
            {
                SnapNow();
            }
        }

        /// <summary>Resolves the provider used by this anchor.</summary>
        public ISnapProvider ResolveProvider()
        {
            if (provider != null)
            {
                return provider;
            }
            if (useProviderOnSameObject && cachedProvider != null)
            {
                return cachedProvider;
            }
            return null;
        }

        /// <summary>Overrides the provider for runtime driven scenarios.</summary>
        public void SetProvider(ISnapProvider newProvider)
        {
            provider = newProvider;
        }

        /// <summary>Snaps the transform to the configured lattice.</summary>
        public void SnapNow()
        {
            ISnapProvider active = ResolveProvider();
            if (active != null && active.Enabled)
            {
                SnapResult result = active.Snap(transform.position, transform.rotation, transform.localScale);
                transform.position = result.Position;
                transform.rotation = result.Rotation;
                transform.localScale = result.Scale;
                return;
            }

            if (profile != null)
            {
                SnapWithProfile();
            }
        }

        private void SnapWithProfile()
        {
            Vector3 worldOffset = transform.TransformVector(localOffset);
            Vector3 position = transform.position + worldOffset;

            Vector3 snapped = SnapQuantizer.QuantizeMasked(position, profile.PositionStep, profile.PositionAxes);
            if (profile.GridSettings != null)
            {
                snapped = GridMath.SnapToCellCenter(position, profile.GridSettings);
            }

            transform.position = snapped - worldOffset;

            if (profile.SnapRotation)
            {
                transform.rotation = SnapQuantizer.Quantize(transform.rotation, profile.RotationStep);
            }

            if (profile.SnapScale)
            {
                transform.localScale = SnapQuantizer.Quantize(transform.localScale, profile.ScaleStep);
            }
        }
    }
}
