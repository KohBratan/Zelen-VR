using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Stateless smoothing and resampling helpers for stroke point buffers.
    /// All methods return new lists and never mutate their inputs.
    /// </summary>
    public static class StrokeSmoother
    {
        /// <summary>Simple centred moving average with a configurable window.</summary>
        public static List<Vector3> MovingAverage(IReadOnlyList<Vector3> points, int window)
        {
            List<Vector3> result = new List<Vector3>();
            if (points == null || points.Count == 0)
            {
                return result;
            }

            int radius = Mathf.Max(0, window / 2);
            if (radius == 0)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    result.Add(points[i]);
                }
                return result;
            }

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 sum = Vector3.zero;
                int count = 0;
                int from = Mathf.Max(0, i - radius);
                int to = Mathf.Min(points.Count - 1, i + radius);
                for (int j = from; j <= to; j++)
                {
                    sum += points[j];
                    count++;
                }
                result.Add(count > 0 ? sum / count : points[i]);
            }
            return result;
        }

        /// <summary>Exponential smoothing that preserves the first and last point.</summary>
        public static List<Vector3> Exponential(IReadOnlyList<Vector3> points, float alpha)
        {
            List<Vector3> result = new List<Vector3>();
            if (points == null || points.Count == 0)
            {
                return result;
            }

            float a = Mathf.Clamp01(alpha);
            Vector3 current = points[0];
            result.Add(current);
            for (int i = 1; i < points.Count; i++)
            {
                current = Vector3.Lerp(current, points[i], a);
                result.Add(current);
            }
            return result;
        }

        /// <summary>
        /// Resamples a polyline at approximately equal arc-length intervals.
        /// Useful before rasterization so thick strokes do not leave gaps.
        /// </summary>
        public static List<Vector3> Resample(IReadOnlyList<Vector3> points, float step)
        {
            List<Vector3> result = new List<Vector3>();
            if (points == null || points.Count == 0)
            {
                return result;
            }
            if (points.Count == 1)
            {
                result.Add(points[0]);
                return result;
            }

            float spacing = Mathf.Max(0.0001f, step);
            result.Add(points[0]);
            float carried = 0f;

            for (int i = 1; i < points.Count; i++)
            {
                Vector3 a = points[i - 1];
                Vector3 b = points[i];
                float segmentLength = Vector3.Distance(a, b);
                if (segmentLength <= 0.000001f)
                {
                    continue;
                }

                float travelled = spacing - carried;
                while (travelled <= segmentLength)
                {
                    float t = travelled / segmentLength;
                    result.Add(Vector3.Lerp(a, b, t));
                    travelled += spacing;
                }
                carried = segmentLength - (travelled - spacing);
                if (carried < 0f)
                {
                    carried = 0f;
                }
            }

            if (result.Count == 0 || (result[result.Count - 1] - points[points.Count - 1]).sqrMagnitude > 0.0000001f)
            {
                result.Add(points[points.Count - 1]);
            }
            return result;
        }

        /// <summary>Catmull-Rom interpolation between control points.</summary>
        public static List<Vector3> CatmullRom(IReadOnlyList<Vector3> controlPoints, int subdivisions)
        {
            List<Vector3> result = new List<Vector3>();
            if (controlPoints == null || controlPoints.Count == 0)
            {
                return result;
            }
            if (controlPoints.Count < 3 || subdivisions <= 0)
            {
                for (int i = 0; i < controlPoints.Count; i++)
                {
                    result.Add(controlPoints[i]);
                }
                return result;
            }

            int steps = Mathf.Max(1, subdivisions);
            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                Vector3 p0 = controlPoints[Mathf.Max(0, i - 1)];
                Vector3 p1 = controlPoints[i];
                Vector3 p2 = controlPoints[i + 1];
                Vector3 p3 = controlPoints[Mathf.Min(controlPoints.Count - 1, i + 2)];

                for (int s = 0; s < steps; s++)
                {
                    float t = (float)s / steps;
                    result.Add(CatmullRomPoint(p0, p1, p2, p3, t));
                }
            }

            result.Add(controlPoints[controlPoints.Count - 1]);
            return result;
        }

        private static Vector3 CatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * ((2f * p1)
                + (-p0 + p2) * t
                + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
                + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        /// <summary>Projects every point onto the grid plane described by settings.</summary>
        public static List<Vector3> ProjectToPlane(IReadOnlyList<Vector3> points, GridSettings settings)
        {
            List<Vector3> result = new List<Vector3>();
            if (points == null)
            {
                return result;
            }
            for (int i = 0; i < points.Count; i++)
            {
                result.Add(GridMath.ProjectOntoPlane(points[i], settings));
            }
            return result;
        }
    }
}
