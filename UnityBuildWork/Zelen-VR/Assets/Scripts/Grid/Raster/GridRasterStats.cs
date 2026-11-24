using System;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Lightweight diagnostics accumulator for rasterization. Instances are
    /// cheap and can be enabled per stroke without affecting gameplay.
    /// </summary>
    [Serializable]
    public class GridRasterStats
    {
        [SerializeField] private int strokeCount;
        [SerializeField] private int centreCells;
        [SerializeField] private int paintedCells;
        [SerializeField] private int skippedCells;
        [SerializeField] private int duplicateCells;
        [SerializeField] private float totalLength;
        [SerializeField] private float lastStrokeDuration;

        public int StrokeCount { get { return strokeCount; } }
        public int CentreCells { get { return centreCells; } }
        public int PaintedCells { get { return paintedCells; } }
        public int SkippedCells { get { return skippedCells; } }
        public int DuplicateCells { get { return duplicateCells; } }
        public float TotalLength { get { return totalLength; } }
        public float LastStrokeDuration { get { return lastStrokeDuration; } }

        public void Reset()
        {
            strokeCount = 0;
            centreCells = 0;
            paintedCells = 0;
            skippedCells = 0;
            duplicateCells = 0;
            totalLength = 0f;
            lastStrokeDuration = 0f;
        }

        public void RecordStroke(DrawStroke stroke, int centreCellCount, int painted, int skipped, int duplicates, float duration)
        {
            strokeCount++;
            centreCells += Mathf.Max(0, centreCellCount);
            paintedCells += Mathf.Max(0, painted);
            skippedCells += Mathf.Max(0, skipped);
            duplicateCells += Mathf.Max(0, duplicates);
            totalLength += stroke != null ? stroke.Length : 0f;
            lastStrokeDuration = Mathf.Max(0f, duration);
        }

        public string ToSummary()
        {
            return "strokes=" + strokeCount
                + " centres=" + centreCells
                + " painted=" + paintedCells
                + " skipped=" + skippedCells
                + " duplicates=" + duplicateCells
                + " length=" + totalLength.ToString("F2")
                + " last=" + (lastStrokeDuration * 1000f).ToString("F1") + "ms";
        }

        public override string ToString()
        {
            return ToSummary();
        }
    }
}
