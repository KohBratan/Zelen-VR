// ---------------------------------------------------------------------------
// Zelen-VR
// File: DrawInputRouter.cs
// Marshrutizator vvoda.
// ---------------------------------------------------------------------------
using System;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Coordinates available input sources and turns their raw samples into a
    /// single DrawStroke. The router does not rasterize anything; consumers
    /// subscribe to StrokeStarted/StrokeUpdated/StrokeCompleted and decide what
    /// to do with the geometry.
    /// </summary>
    [DisallowMultipleComponent]
    public class DrawInputRouter : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private ControllerDrawInputSource controllerSource;
        [SerializeField] private PointerDrawInputSource pointerSource;
        [SerializeField] private bool preferPointer = true;

        [Header("Stroke")]
        [SerializeField] private float minimumPointDistance = 0.005f;
        [SerializeField] private float simplifyTolerance = 0.002f;
        [SerializeField] private bool smoothOnComplete = true;
        [SerializeField] private int smoothingWindow = 3;
        [SerializeField] private int catmullRomSubdivisions = 0;

        [Header("Debug")]
        [SerializeField] private bool logStrokes = false;

        private IDrawInputSource activeSource;
        private DrawStroke currentStroke;
        private bool strokeActive;

        public DrawStroke CurrentStroke { get { return currentStroke; } }
        public bool IsDrawing { get { return strokeActive; } }
        public IDrawInputSource ActiveSource { get { return activeSource; } }

        public event Action<DrawStroke> StrokeStarted;
        public event Action<DrawStroke> StrokeUpdated;
        public event Action<DrawStroke> StrokeCompleted;

        private void Awake()
        {
            if (controllerSource == null)
            {
                controllerSource = GetComponentInChildren<ControllerDrawInputSource>();
            }
            if (pointerSource == null)
            {
                pointerSource = GetComponentInChildren<PointerDrawInputSource>();
            }
        }

        private void Update()
        {
            IDrawInputSource source = SelectSource();
            if (source == null)
            {
                if (strokeActive)
                {
                    CompleteStroke();
                }
                return;
            }

            if (activeSource != source && strokeActive)
            {
                CompleteStroke();
            }

            activeSource = source;

            if (!strokeActive)
            {
                if (source.WasPressedThisFrame())
                {
                    BeginStroke(source);
                }
                return;
            }

            DrawInputSample sample;
            if (source.TryGetSample(out sample) && sample.IsValid)
            {
                if (currentStroke.AddPoint(sample.WorldPosition, sample.Pressure, minimumPointDistance))
                {
                    if (StrokeUpdated != null)
                    {
                        StrokeUpdated(currentStroke);
                    }
                }
            }

            if (source.WasReleasedThisFrame() || !source.IsDrawing)
            {
                CompleteStroke();
            }
        }

        private IDrawInputSource SelectSource()
        {
            IDrawInputSource first = preferPointer ? (IDrawInputSource)pointerSource : controllerSource;
            IDrawInputSource second = preferPointer ? (IDrawInputSource)controllerSource : pointerSource;

            if (first != null && first.IsAvailable)
            {
                return first;
            }
            if (second != null && second.IsAvailable)
            {
                return second;
            }
            return null;
        }

        public void BeginStroke(IDrawInputSource source)
        {
            if (source == null)
            {
                return;
            }

            activeSource = source;
            currentStroke = new DrawStroke();
            strokeActive = true;
            source.BeginDraw();

            DrawInputSample sample;
            if (source.TryGetSample(out sample) && sample.IsValid)
            {
                currentStroke.AddPoint(sample.WorldPosition, sample.Pressure, 0f);
            }

            if (logStrokes)
            {
                Debug.Log("Draw stroke started by " + source.SourceName, this);
            }

            if (StrokeStarted != null)
            {
                StrokeStarted(currentStroke);
            }
        }

        public void CompleteStroke()
        {
            if (!strokeActive)
            {
                return;
            }

            strokeActive = false;
            if (activeSource != null)
            {
                activeSource.EndDraw();
            }

            DrawStroke finished = currentStroke;
            if (finished != null && finished.Count > 0)
            {
                if (smoothOnComplete && smoothingWindow > 1)
                {
                    DrawStroke smoothed = new DrawStroke();
                    var points = StrokeSmoother.MovingAverage(finished.Points, smoothingWindow);
                    if (catmullRomSubdivisions > 0)
                    {
                        points = StrokeSmoother.CatmullRom(points, catmullRomSubdivisions);
                    }
                    for (int i = 0; i < points.Count; i++)
                    {
                        smoothed.AddPoint(points[i], 1f, 0f);
                    }
                    finished = smoothed;
                }

                if (simplifyTolerance > 0f)
                {
                    finished = finished.Simplify(simplifyTolerance);
                }
            }

            currentStroke = finished;
            if (logStrokes && finished != null)
            {
                Debug.Log("Draw stroke completed with " + finished.Count + " points, length " + finished.Length.ToString("F3"), this);
            }

            if (StrokeCompleted != null && finished != null)
            {
                StrokeCompleted(finished);
            }

            currentStroke = null;
        }

        public void CancelStroke()
        {
            strokeActive = false;
            if (activeSource != null)
            {
                activeSource.EndDraw();
            }
            currentStroke = null;
        }
    }
}
