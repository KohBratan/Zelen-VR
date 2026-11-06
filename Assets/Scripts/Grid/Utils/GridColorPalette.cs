using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZelenVR.Grid
{
    /// <summary>
    /// Named list of colors used by grid layers and brushes. Keeping palette
    /// data in an asset lets artists re-tint the whole grid without touching
    /// code or prefabs.
    /// </summary>
    [CreateAssetMenu(fileName = "GridColorPalette", menuName = "ZelenVR/Grid/Color Palette", order = 2)]
    public class GridColorPalette : ScriptableObject
    {
        [SerializeField] private string paletteName = "Default";
        [SerializeField] private bool loop = true;
        [SerializeField] private List<Color> colors = new List<Color>
        {
            Color.white,
            new Color(0.85f, 0.25f, 0.25f, 1f),
            new Color(0.25f, 0.65f, 0.95f, 1f),
            new Color(0.35f, 0.85f, 0.45f, 1f)
        };

        public string PaletteName
        {
            get { return paletteName; }
            set { paletteName = value; }
        }

        public bool Loop
        {
            get { return loop; }
            set { loop = value; }
        }

        public int Count
        {
            get { return colors != null ? colors.Count : 0; }
        }

        public IReadOnlyList<Color> Colors
        {
            get { return colors; }
        }

        /// <summary>Returns a color by index, wrapping or clamping according to Loop.</summary>
        public Color GetColor(int index)
        {
            return GetColor(index, Color.white);
        }

        public Color GetColor(int index, Color fallback)
        {
            if (colors == null || colors.Count == 0)
            {
                return fallback;
            }
            return colors[NormalizeIndex(index)];
        }

        public int NextIndex(int current)
        {
            if (colors == null || colors.Count == 0)
            {
                return 0;
            }
            if (loop)
            {
                return (NormalizeIndex(current) + 1) % colors.Count;
            }
            return Mathf.Min(current + 1, colors.Count - 1);
        }

        public int PreviousIndex(int current)
        {
            if (colors == null || colors.Count == 0)
            {
                return 0;
            }
            if (loop)
            {
                int next = NormalizeIndex(current) - 1;
                if (next < 0)
                {
                    next += colors.Count;
                }
                return next;
            }
            return Mathf.Max(0, current - 1);
        }

        public void AddColor(Color color)
        {
            EnsureList();
            colors.Add(color);
        }

        public bool RemoveAt(int index)
        {
            EnsureList();
            if (index < 0 || index >= colors.Count || colors.Count <= 1)
            {
                return false;
            }
            colors.RemoveAt(index);
            return true;
        }

        public bool SetColor(int index, Color color)
        {
            EnsureList();
            if (index < 0 || index >= colors.Count)
            {
                return false;
            }
            colors[index] = color;
            return true;
        }

        public int IndexOf(Color color)
        {
            EnsureList();
            for (int i = 0; i < colors.Count; i++)
            {
                if (colors[i] == color)
                {
                    return i;
                }
            }
            return -1;
        }

        public void EnsureMinimum(int minimum)
        {
            EnsureList();
            int target = Mathf.Max(1, minimum);
            while (colors.Count < target)
            {
                colors.Add(Color.white);
            }
        }

        public Color[] ToArray()
        {
            EnsureList();
            return colors.ToArray();
        }

        public void ResetToDefault()
        {
            colors = new List<Color>
            {
                Color.white,
                new Color(0.85f, 0.25f, 0.25f, 1f),
                new Color(0.25f, 0.65f, 0.95f, 1f),
                new Color(0.35f, 0.85f, 0.45f, 1f)
            };
        }

        private int NormalizeIndex(int index)
        {
            if (colors == null || colors.Count == 0)
            {
                return 0;
            }
            if (loop)
            {
                int wrapped = index % colors.Count;
                return wrapped < 0 ? wrapped + colors.Count : wrapped;
            }
            return Mathf.Clamp(index, 0, colors.Count - 1);
        }

        private void EnsureList()
        {
            if (colors == null)
            {
                colors = new List<Color>();
            }
        }

        private void OnValidate()
        {
            EnsureList();
            if (colors.Count == 0)
            {
                colors.Add(Color.white);
            }
        }
    }
}
