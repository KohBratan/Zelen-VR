using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Mutable buffer of world-space points that belong to one continuous
    /// stroke. The class tracks a bounding box and approximate length so the
    /// rasterizer can reject off-grid strokes cheaply.
    /// </summary>
    [Serializable]
    public class DrawStroke
    {
        [SerializeField] private List<Vector3> points = new List<Vector3>();
        [SerializeField] private List<float> pressures = new List<float>();
        [SerializeField] private Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        [SerializeField] private float length;
        [SerializeField] private float startedAt;
        [SerializeField] private float updatedAt;

        public IReadOnlyList<Vector3> Points { get { return points; } }
        public IReadOnlyList<float> Pressures { get { return pressures; } }
        public Bounds Bounds { get { return bounds; } }
        public float Length { get { return length; } }
        public float StartedAt { get { return startedAt; } }
        public float UpdatedAt { get { return updatedAt; } }
        public int Count { get { return points.Count; } }
        public bool IsEmpty { get { return points.Count == 0; } }

        public Vector3 First
        {
            get { return points.Count > 0 ? points[0] : Vector3.zero; }
        }

        public Vector3 Last
        {
            get { return points.Count > 0 ? points[points.Count - 1] : Vector3.zero; }
        }

        public DrawStroke()
        {
            startedAt = Time.time;
            updatedAt = startedAt;
        }

        public void Clear()
        {
            points.Clear();
            pressures.Clear();
            bounds = new Bounds(Vector3.zero, Vector3.zero);
            length = 0f;
            startedAt = Time.time;
            updatedAt = startedAt;
        }

        /// <summary>
        /// Appends a point. Returns false when the point is too close to the
        /// previous one, which keeps the buffer compact under noisy input.
        /// </summary>
        public bool AddPoint(Vector3 point, float pressure = 1f, float minDistance = 0.001f)
        {
            if (points.Count > 0)
            {
                float sqr = (point - points[points.Count - 1]).sqrMagnitude;
                if (sqr < minDistance * minDistance)
                {
                    return false;
                }
                length += Mathf.Sqrt(sqr);
            }

            points.Add(point);
            pressures.Add(Mathf.Clamp01(pressure));
            updatedAt = Time.time;
            RecalculateBoundsFromPoint(point);
            return true;
        }

        public void AddRange(IEnumerable<Vector3> source)
        {
            if (source == null)
            {
                return;
            }
            foreach (Vector3 point in source)
            {
                AddPoint(point, 1f, 0f);
            }
        }

        private void RecalculateBoundsFromPoint(Vector3 point)
        {
            if (points.Count == 1)
            {
                bounds = new Bounds(point, Vector3.zero);
                return;
            }
            bounds.Encapsulate(point);
        }

        public void RecalculateBounds()
        {
            if (points.Count == 0)
            {
                bounds = new Bounds(Vector3.zero, Vector3.zero);
                return;
            }

            Bounds result = new Bounds(points[0], Vector3.zero);
            for (int i = 1; i < points.Count; i++)
            {
                result.Encapsulate(points[i]);
            }
            bounds = result;
        }

        /// <summary>Returns a simplified copy using the Ramer-Douglas-Peucker algorithm.</summary>
        public DrawStroke Simplify(float tolerance)
        {
            DrawStroke result = new DrawStroke();
            if (points.Count == 0)
            {
                return result;
            }

            List<Vector3> simplified = SimplifyPoints(points, Mathf.Max(0f, tolerance));
            for (int i = 0; i < simplified.Count; i++)
            {
                result.AddPoint(simplified[i], 1f, 0f);
            }
            return result;
        }

        private static List<Vector3> SimplifyPoints(List<Vector3> source, float tolerance)
        {
            if (source.Count <= 2 || tolerance <= 0f)
            {
                return new List<Vector3>(source);
            }

            bool[] keep = new bool[source.Count];
            keep[0] = true;
            keep[source.Count - 1] = true;
            SimplifySection(source, 0, source.Count - 1, tolerance * tolerance, keep);

            List<Vector3> result = new List<Vector3>();
            for (int i = 0; i < source.Count; i++)
            {
                if (keep[i])
                {
                    result.Add(source[i]);
                }
            }
            return result;
        }

        private static void SimplifySection(List<Vector3> source, int first, int last, float sqrTolerance, bool[] keep)
        {
            if (last <= first + 1)
            {
                return;
            }

            Vector3 a = source[first];
            Vector3 b = source[last];
            float maxDistance = -1f;
            int maxIndex = -1;

            for (int i = first + 1; i < last; i++)
            {
                float distance = SqrDistanceToSegment(source[i], a, b);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxIndex = i;
                }
            }

            if (maxDistance > sqrTolerance && maxIndex >= 0)
            {
                keep[maxIndex] = true;
                SimplifySection(source, first, maxIndex, sqrTolerance, keep);
                SimplifySection(source, maxIndex, last, sqrTolerance, keep);
            }
        }

        private static float SqrDistanceToSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float lengthSqr = ab.sqrMagnitude;
            if (lengthSqr <= 0.000001f)
            {
                return (point - a).sqrMagnitude;
            }

            float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / lengthSqr);
            Vector3 projection = a + ab * t;
            return (point - projection).sqrMagnitude;
        }
    }
}
