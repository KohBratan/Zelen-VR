using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZelenVR.Grid
{
    public enum GridValidationSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    /// <summary>One diagnostic produced while validating grid configuration or data.</summary>
    [Serializable]
    public struct GridValidationIssue
    {
        public GridValidationSeverity Severity;
        public string Message;
        public string Context;

        public GridValidationIssue(GridValidationSeverity severity, string message, string context)
        {
            Severity = severity;
            Message = message;
            Context = context;
        }

        public override string ToString()
        {
            string prefix = Severity.ToString().ToUpperInvariant();
            return string.IsNullOrEmpty(Context)
                ? "[" + prefix + "] " + Message
                : "[" + prefix + "] " + Context + ": " + Message;
        }
    }

    /// <summary>Accumulates validation issues and exposes aggregate counts.</summary>
    public class GridValidationReport
    {
        private readonly List<GridValidationIssue> issues = new List<GridValidationIssue>();

        public IReadOnlyList<GridValidationIssue> Issues { get { return issues; } }
        public int Count { get { return issues.Count; } }

        public bool HasErrors
        {
            get
            {
                for (int i = 0; i < issues.Count; i++)
                {
                    if (issues[i].Severity == GridValidationSeverity.Error)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public int ErrorCount { get { return CountSeverity(GridValidationSeverity.Error); } }
        public int WarningCount { get { return CountSeverity(GridValidationSeverity.Warning); } }

        public void Add(GridValidationIssue issue)
        {
            issues.Add(issue);
        }

        public void Add(GridValidationSeverity severity, string message, string context)
        {
            issues.Add(new GridValidationIssue(severity, message, context));
        }

        public void AddError(string message, string context)
        {
            Add(GridValidationSeverity.Error, message, context);
        }

        public void AddWarning(string message, string context)
        {
            Add(GridValidationSeverity.Warning, message, context);
        }

        public void Merge(GridValidationReport other)
        {
            if (other == null)
            {
                return;
            }
            for (int i = 0; i < other.issues.Count; i++)
            {
                issues.Add(other.issues[i]);
            }
        }

        public string ToSummary()
        {
            return "issues=" + issues.Count + " errors=" + ErrorCount + " warnings=" + WarningCount;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Grid validation: ");
            builder.Append(ToSummary());
            for (int i = 0; i < issues.Count; i++)
            {
                builder.AppendLine();
                builder.Append(issues[i].ToString());
            }
            return builder.ToString();
        }

        private int CountSeverity(GridValidationSeverity severity)
        {
            int count = 0;
            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i].Severity == severity)
                {
                    count++;
                }
            }
            return count;
        }
    }

    /// <summary>
    /// Pure validation routines for settings assets, layers and canvases. The
    /// methods never log directly; callers decide how to present the report.
    /// </summary>
    public static class GridValidation
    {
        public static GridValidationReport ValidateSettings(GridSettings settings)
        {
            GridValidationReport report = new GridValidationReport();
            if (settings == null)
            {
                report.AddError("GridSettings is null.", "settings");
                return report;
            }

            if (settings.Width <= 0)
            {
                report.AddError("Width must be positive.", "settings.width");
            }
            if (settings.Height <= 0)
            {
                report.AddError("Height must be positive.", "settings.height");
            }
            if (settings.CellSize <= 0f)
            {
                report.AddError("Cell size must be positive.", "settings.cellSize");
            }
            if (settings.ChunkSize <= 0)
            {
                report.AddError("Chunk size must be positive.", "settings.chunkSize");
            }
            if ((long)settings.Width * settings.Height > int.MaxValue)
            {
                report.AddWarning("Cell count exceeds Int32 range.", "settings.dimensions");
            }
            if (!GridUtils.IsFinite(settings.Origin))
            {
                report.AddError("Origin contains NaN or infinity.", "settings.origin");
            }
            if (settings.Width > 256 || settings.Height > 256)
            {
                report.AddWarning("Large grid may allocate many cells and line renderers.", "settings.dimensions");
            }
            if (settings.CellSize > 4f)
            {
                report.AddWarning("Very large cells may make snapping feel coarse.", "settings.cellSize");
            }
            return report;
        }

        public static GridValidationReport ValidateLayer(GridLayer layer, GridSettings settings)
        {
            GridValidationReport report = new GridValidationReport();
            if (layer == null)
            {
                report.AddError("GridLayer is null.", "layer");
                return report;
            }

            if (settings == null)
            {
                report.AddWarning("No GridSettings supplied; cell bounds cannot be validated.", layer.Name);
            }

            foreach (KeyValuePair<GridCoordinate, GridCell> pair in layer.EnumerateCells())
            {
                GridCoordinate coordinate = pair.Key;
                GridCell cell = pair.Value;

                if (settings != null && !settings.Contains(coordinate))
                {
                    report.AddError("Cell lies outside grid bounds at " + GridUtils.FormatCoordinate(coordinate) + ".", layer.Name);
                }
                if (cell.Coordinate != coordinate)
                {
                    report.AddError("Cell key does not match stored coordinate at " + GridUtils.FormatCoordinate(coordinate) + ".", layer.Name);
                }
                if (cell.Layer != layer.Index)
                {
                    report.AddWarning("Cell layer index " + cell.Layer + " differs from layer " + layer.Index + ".", layer.Name);
                }
                if (cell.Fill < 0f || cell.Fill > 1f)
                {
                    report.AddWarning("Cell fill is outside [0,1] at " + GridUtils.FormatCoordinate(coordinate) + ".", layer.Name);
                }
                if (cell.IsEmpty)
                {
                    report.AddWarning("Empty cell stored in layer at " + GridUtils.FormatCoordinate(coordinate) + ".", layer.Name);
                }
            }

            if (layer.Locked && layer.Visible)
            {
                report.Add(GridValidationSeverity.Info, "Layer is locked but visible.", layer.Name);
            }
            return report;
        }

        public static GridValidationReport ValidateCanvas(GridCanvas canvas)
        {
            GridValidationReport report = new GridValidationReport();
            if (canvas == null)
            {
                report.AddError("GridCanvas is null.", "canvas");
                return report;
            }

            if (canvas.Settings == null)
            {
                report.AddWarning("Canvas has no GridSettings assigned.", "canvas.settings");
            }

            for (int i = 0; i < canvas.LayerCount; i++)
            {
                report.Merge(ValidateLayer(canvas.GetLayer(i), canvas.Settings));
            }

            if (canvas.LayerCount == 0)
            {
                report.AddWarning("Canvas has no layers.", "canvas.layers");
            }
            return report;
        }
    }
}
