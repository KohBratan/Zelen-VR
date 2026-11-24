using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Mouse and touch drawing source. Screen positions are converted to rays
    /// through the assigned camera and intersected with the grid plane; when a
    /// collider is present on the grid layer it takes precedence.
    /// </summary>
    [DisallowMultipleComponent]
    public class PointerDrawInputSource : MonoBehaviour, IDrawInputSource
    {
        [Header("Camera")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private bool useMainCameraIfMissing = true;

        [Header("Grid")]
        [SerializeField] private GridSettings settings;
        [SerializeField] private LayerMask rayMask = ~0;
        [SerializeField] private float rayLength = 1000f;

        [Header("Input")]
        [SerializeField] private bool allowMouse = true;
        [SerializeField] private bool allowTouch = true;
        [SerializeField] private int mouseButton = 0;

        private bool drawing;

        public string SourceName { get { return "Pointer"; } }
        public bool IsAvailable { get { return ResolveCamera() != null; } }
        public bool IsDrawing { get { return drawing; } }
        public GridSettings Settings { get { return settings; } set { settings = value; } }

        private Camera ResolveCamera()
        {
            if (targetCamera != null)
            {
                return targetCamera;
            }
            if (useMainCameraIfMissing)
            {
                targetCamera = Camera.main;
            }
            return targetCamera;
        }

        public bool TryGetSample(out DrawInputSample sample)
        {
            Camera cam = ResolveCamera();
            if (cam == null)
            {
                sample = DrawInputSample.Invalid;
                return false;
            }

            Vector2 screenPosition;
            if (!TryGetScreenPosition(out screenPosition))
            {
                sample = DrawInputSample.Invalid;
                return false;
            }

            Ray ray = cam.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Max(0.01f, rayLength), rayMask, QueryTriggerInteraction.Ignore))
            {
                sample = new DrawInputSample(hit.point, hit.normal, 1f, Time.time);
                return true;
            }

            Vector3 planePoint;
            if (TryIntersectGridPlane(ray, out planePoint))
            {
                sample = new DrawInputSample(planePoint, settings != null ? settings.PlaneNormal : Vector3.up, 1f, Time.time);
                return true;
            }

            sample = DrawInputSample.Invalid;
            return false;
        }

        private bool TryGetScreenPosition(out Vector2 position)
        {
            if (allowTouch && Input.touchCount > 0)
            {
                position = Input.GetTouch(0).position;
                return true;
            }

            if (allowMouse)
            {
                position = Input.mousePosition;
                return true;
            }

            position = Vector2.zero;
            return false;
        }

        private bool TryIntersectGridPlane(Ray ray, out Vector3 point)
        {
            point = Vector3.zero;
            if (settings == null)
            {
                return false;
            }

            Vector3 planePoint = GridMath.GetCellZeroCorner(settings);
            Vector3 normal = settings.PlaneNormal;
            float denominator = Vector3.Dot(ray.direction, normal);
            if (Mathf.Abs(denominator) < 0.00001f)
            {
                return false;
            }

            float distance = Vector3.Dot(planePoint - ray.origin, normal) / denominator;
            if (distance < 0f)
            {
                return false;
            }

            point = ray.origin + ray.direction * distance;
            return true;
        }

        public bool WasPressedThisFrame()
        {
            if (allowTouch && Input.touchCount > 0)
            {
                return Input.GetTouch(0).phase == TouchPhase.Began;
            }
            return allowMouse && Input.GetMouseButtonDown(mouseButton);
        }

        public bool WasReleasedThisFrame()
        {
            if (allowTouch && Input.touchCount > 0)
            {
                TouchPhase phase = Input.GetTouch(0).phase;
                return phase == TouchPhase.Ended || phase == TouchPhase.Canceled;
            }
            return allowMouse && Input.GetMouseButtonUp(mouseButton);
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
