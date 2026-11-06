using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Converts continuous stroke geometry into discrete grid cells. Two line
    /// algorithms are provided: a classic Bresenham plotter and a supercover
    /// variant that also includes cells whose edges are crossed diagonally.
    /// </summary>
    public static class GridRasterizer
    {
        /// <summary>Bresenham line between two cell coordinates (inclusive).</summary>
        public static void RasterizeLine(GridCoordinate from, GridCoordinate to, List<GridCoordinate> output)
        {
            if (output == null)
            {
                return;
            }

            int x0 = from.X;
            int y0 = from.Y;
            int x1 = to.X;
            int y1 = to.Y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                output.Add(new GridCoordinate(x0, y0));
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }
                int e2 = err * 2;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        /// <summary>
        /// Supercover line: every cell whose area is touched by the segment is
        /// returned. Produces a thicker, gap-free result when drawing quickly.
        /// </summary>
        public static void RasterizeSupercover(GridCoordinate from, GridCoordinate to, List<GridCoordinate> output)
        {
            if (output == null)
            {
                return;
            }

            int x0 = from.X;
            int y0 = from.Y;
            int x1 = to.X;
            int y1 = to.Y;

            int dx = x1 - x0;
            int dy = y1 - y0;
            int nx = Mathf.Abs(dx);
            int ny = Mathf.Abs(dy);
            int signX = dx >= 0 ? 1 : -1;
            int signY = dy >= 0 ? 1 : -1;
            int ix = 0;
            int iy = 0;

            output.Add(new GridCoordinate(x0, y0));

            while (ix < nx || iy < ny)
            {
                int decision = (1 + 2 * ix) * ny - (1 + 2 * iy) * nx;
                if (decision == 0)
                {
                    x0 += signX;
                    y0 += signY;
                    ix++;
                    iy++;
                    output.Add(new GridCoordinate(x0, y0));
                }
                else if (decision < 0)
                {
                    x0 += signX;
                    ix++;
                    output.Add(new GridCoordinate(x0, y0));
                }
                else
                {
                    y0 += signY;
                    iy++;
                    output.Add(new GridCoordinate(x0, y0));
                }
            }
        }

        /// <summary>Rasterizes a stroke into raw centre cells using Bresenham.</summary>
        public static List<GridCoordinate> RasterizeStroke(DrawStroke stroke, GridSettings settings, bool useSupercover)
        {
            List<GridCoordinate> result = new List<GridCoordinate>();
            if (stroke == null || settings == null || stroke.Count == 0)
            {
                return result;
            }

            GridCoordinate previous = GridMath.WorldToGrid(stroke.Points[0], settings, true);
            result.Add(previous);
            for (int i = 1; i < stroke.Count; i++)
            {
                GridCoordinate current = GridMath.WorldToGrid(stroke.Points[i], settings, true);
                if (current == previous)
                {
                    continue;
                }

                List<GridCoordinate> segment = new List<GridCoordinate>();
                if (useSupercover)
                {
                    RasterizeSupercover(previous, current, segment);
                }
                else
                {
                    RasterizeLine(previous, current, segment);
                }

                for (int s = 0; s < segment.Count; s++)
                {
                    result.Add(segment[s]);
                }
                previous = current;
            }
            return result;
        }

        /// <summary>
        /// Applies a brush footprint around every centre cell, returning the
        /// final set of affected cells (duplicates removed).
        /// </summary>
        public static HashSet<GridCoordinate> ApplyBrush(IEnumerable<GridCoordinate> centres, GridBrush brush, GridSettings settings)
        {
            HashSet<GridCoordinate> result = new HashSet<GridCoordinate>();
            if (centres == null || settings == null)
            {
                return result;
            }

            foreach (GridCoordinate centre in centres)
            {
                foreach (GridCoordinate offset in brush.GetOffsets())
                {
                    GridCoordinate target = centre + offset;
                    if (GridMath.Contains(target, settings))
                    {
                        result.Add(target);
                    }
                }
            }
            return result;
        }

        /// <summary>Fills a rectangular region (inclusive) into the output set.</summary>
        public static void RasterizeRect(GridCoordinate min, GridCoordinate max, HashSet<GridCoordinate> output)
        {
            if (output == null)
            {
                return;
            }
            for (int y = min.Y; y <= max.Y; y++)
            {
                for (int x = min.X; x <= max.X; x++)
                {
                    output.Add(new GridCoordinate(x, y));
                }
            }
        }

        /// <summary>Rasterizes a circle outline or disc depending on filled.</summary>
        public static void RasterizeCircle(GridCoordinate centre, int radius, bool filled, HashSet<GridCoordinate> output)
        {
            if (output == null)
            {
                return;
            }

            int r = Mathf.Max(0, radius);
            int x = r;
            int y = 0;
            int err = 0;

            while (x >= y)
            {
                if (filled)
                {
                    AddHorizontalSpan(centre, x, y, output);
                    AddHorizontalSpan(centre, y, x, output);
                }
                else
                {
                    output.Add(new GridCoordinate(centre.X + x, centre.Y + y));
                    output.Add(new GridCoordinate(centre.X + y, centre.Y + x));
                    output.Add(new GridCoordinate(centre.X - y, centre.Y + x));
                    output.Add(new GridCoordinate(centre.X - x, centre.Y + y));
                    output.Add(new GridCoordinate(centre.X - x, centre.Y - y));
                    output.Add(new GridCoordinate(centre.X - y, centre.Y - x));
                    output.Add(new GridCoordinate(centre.X + y, centre.Y - x));
                    output.Add(new GridCoordinate(centre.X + x, centre.Y - y));
                }

                y++;
                if (err <= 0)
                {
                    err += 2 * y + 1;
                }
                if (err > 0)
                {
                    x--;
                    err -= 2 * x + 1;
                }
            }
        }

        private static void AddHorizontalSpan(GridCoordinate centre, int halfWidth, int offsetY, HashSet<GridCoordinate> output)
        {
            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                output.Add(new GridCoordinate(centre.X + x, centre.Y + offsetY));
            }
        }
    }
}
