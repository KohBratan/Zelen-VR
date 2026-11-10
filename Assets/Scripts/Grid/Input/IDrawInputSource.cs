using System;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// One immutable sample produced by a drawing input source. The sample is
    /// already expressed in world space and projected onto the grid plane when
    /// the source supports plane projection.
    /// </summary>
    [Serializable]
    public struct DrawInputSample
    {
        public Vector3 WorldPosition;
        public Vector3 WorldNormal;
        public float Pressure;
        public float Time;
        public bool IsValid;

        public DrawInputSample(Vector3 worldPosition, Vector3 worldNormal, float pressure, float time)
        {
            WorldPosition = worldPosition;
            WorldNormal = worldNormal;
            Pressure = Mathf.Clamp01(pressure);
            Time = time;
            IsValid = true;
        }

        public static DrawInputSample Invalid
        {
            get
            {
                DrawInputSample sample = new DrawInputSample();
                sample.IsValid = false;
                return sample;
            }
        }
    }

    /// <summary>
    /// Abstraction over anything that can produce drawing samples: XR
    /// controllers, mouse, touch, replay tools or automated tests.
    /// </summary>
    public interface IDrawInputSource
    {
        string SourceName { get; }
        bool IsAvailable { get; }
        bool IsDrawing { get; }

        bool TryGetSample(out DrawInputSample sample);

        bool WasPressedThisFrame();
        bool WasReleasedThisFrame();

        void BeginDraw();
        void EndDraw();
    }
}
