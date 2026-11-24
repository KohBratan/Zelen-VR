using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// XR-style drawing source. A serialized transform represents the tip of a
    /// controller or stylus; a short raycast against the configured layer mask
    /// produces the drawing position. When nothing is hit, the tip is projected
    /// onto the mathematical grid plane so drawing still works in empty space.
    /// </summary>
    [DisallowMultipleComponent]
    public class ControllerDrawInputSource : MonoBehaviour, IDrawInputSource
    {
        [Header("Tip")]
        [SerializeField] private Transform tip;
        [SerializeField] private float rayLength = 3f;
        [SerializeField] private LayerMask rayMask = ~0;

        [Header("Grid")]
        [SerializeField] private GridSettings settings;

        [Header("Behaviour")]
        [SerializeField] private bool useTriggerButton = true;
        [SerializeField] private string triggerAxis = "Fire1";
        [SerializeField] private float pressure = 1f;

        private bool drawing;

        public string SourceName { get { return "Controller"; } }
        public bool IsAvailable { get { return tip != null; } }
        public bool IsDrawing { get { return drawing; } }
        public Transform Tip { get { return tip; } set { tip = value; } }
        public GridSettings Settings { get { return settings; } set { settings = value; } }

        public bool TryGetSample(out DrawInputSample sample)
        {
            if (tip == null)
            {
                sample = DrawInputSample.Invalid;
                return false;
            }

            Vector3 origin = tip.position;
            Vector3 direction = tip.forward;
            if (direction.sqrMagnitude < 0.000001f)
            {
                direction = Vector3.forward;
            }

            Ray ray = new Ray(origin, direction.normalized);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Max(0.01f, rayLength), rayMask, QueryTriggerInteraction.Ignore))
            {
                sample = new DrawInputSample(hit.point, hit.normal, pressure, Time.time);
                return true;
            }

            Vector3 projected = settings != null
                ? GridMath.ProjectOntoPlane(origin, settings)
                : origin + direction * rayLength;
            sample = new DrawInputSample(projected, settings != null ? settings.PlaneNormal : Vector3.up, pressure, Time.time);
            return true;
        }

        public bool WasPressedThisFrame()
        {
            if (!useTriggerButton)
            {
                return false;
            }
            return Input.GetButtonDown(triggerAxis);
        }

        public bool WasReleasedThisFrame()
        {
            if (!useTriggerButton)
            {
                return false;
            }
            return Input.GetButtonUp(triggerAxis);
        }

        public void BeginDraw()
        {
            drawing = true;
        }

        public void EndDraw()
        {
            drawing = false;
        }
    }
}
