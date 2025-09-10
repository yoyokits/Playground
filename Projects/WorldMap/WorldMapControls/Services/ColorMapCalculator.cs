// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using WorldMapControls.Models.Enums;

    /// <summary>
    /// Calculates colors based on data values using various colormap types.
    /// </summary>
    public static class ColorMapCalculator
    {
        #region Methods

        /// <summary>
        /// Gets a color at a specific position (0.0 to 1.0) in the colormap.
        /// </summary>
        /// <param name="position">Position in colormap (0.0 = bottom, 1.0 = top).</param>
        /// <param name="colorMapType">Type of colormap.</param>
        /// <returns>Color at the specified position.</returns>
        public static Color GetColorAtPosition(double position, ColorMapType colorMapType)
        {
            position = Math.Clamp(position, 0.0, 1.0);

            return colorMapType switch
            {
                ColorMapType.Jet => GetJetColor(position),
                ColorMapType.HSV => GetHSVColor(position),
                ColorMapType.Autumn => GetAutumnColor(position),
                ColorMapType.Winter => GetWinterColor(position),
                ColorMapType.Spring => GetSpringColor(position),
                ColorMapType.Summer => GetSummerColor(position),
                ColorMapType.Gray => GetGrayColor(position),
                ColorMapType.Hot => GetHotColor(position),
                ColorMapType.Cool => GetCoolColor(position),
                ColorMapType.Viridis => GetViridisColor(position),
                ColorMapType.Plasma => GetPlasmaColor(position),
                ColorMapType.Inferno => GetInfernoColor(position),
                ColorMapType.Magma => GetMagmaColor(position),
                ColorMapType.Blues => GetBluesColor(position),
                ColorMapType.Reds => GetRedsColor(position),
                ColorMapType.Greens => GetGreensColor(position),
                ColorMapType.Oranges => GetOrangesColor(position),
                ColorMapType.Purples => GetPurplesColor(position),
                ColorMapType.RdBu => GetRdBuColor(position),
                ColorMapType.Spectral => GetSpectralColor(position),
                ColorMapType.Rainbow => GetRainbowColor(position),
                _ => GetJetColor(position)
            };
        }

        /// <summary>
        /// Maps a list of values to colors using the specified colormap.
        /// The minimum value maps to the bottom color, maximum to the top color.
        /// </summary>
        /// <param name="values">Input values to map.</param>
        /// <param name="colorMapType">Type of colormap to use.</param>
        /// <returns>Array of colors corresponding to input values.</returns>
        public static Color[] MapValues(IEnumerable<double> values, ColorMapType colorMapType)
        {
            var valueArray = values.ToArray();
            if (valueArray.Length == 0) return Array.Empty<Color>();

            var min = valueArray.Min();
            var max = valueArray.Max();
            var range = max - min;

            if (Math.Abs(range) < double.Epsilon)
            {
                // All values are the same, return middle color
                var singleColor = GetColorAtPosition(0.5, colorMapType);
                return Enumerable.Repeat(singleColor, valueArray.Length).ToArray();
            }

            return valueArray.Select(value =>
            {
                var normalizedValue = (value - min) / range;
                return GetColorAtPosition(normalizedValue, colorMapType);
            }).ToArray();
        }

        private static Color GetAutumnColor(double t)
        {
            // Red to yellow
            var r = 255;
            var g = (byte)(t * 255);
            var b = 0;
            return Color.FromRgb((byte)r, g, (byte)b);
        }

        private static Color GetBluesColor(double t)
        {
            return InterpolateRGB(247, 251, 255, 8, 48, 107, t);
        }

        private static Color GetCoolColor(double t)
        {
            // Cyan to magenta
            var r = (byte)(t * 255);
            var g = (byte)(255 - t * 255);
            var b = 255;
            return Color.FromRgb(r, g, (byte)b);
        }

        private static Color GetGrayColor(double t)
        {
            // Black to white
            var intensity = (byte)(t * 255);
            return Color.FromRgb(intensity, intensity, intensity);
        }

        private static Color GetGreensColor(double t)
        {
            return InterpolateRGB(247, 252, 245, 0, 68, 27, t);
        }

        private static Color GetHotColor(double t)
        {
            // Black -> red -> yellow -> white
            if (t < 0.33)
            {
                var r = (byte)(t * 3 * 255);
                return Color.FromRgb(r, 0, 0);
            }
            if (t < 0.66)
            {
                var g = (byte)((t - 0.33) * 3 * 255);
                return Color.FromRgb(255, g, 0);
            }
            var b = (byte)((t - 0.66) * 3 * 255);
            return Color.FromRgb(255, 255, b);
        }

        private static Color GetHSVColor(double t)
        {
            // HSV colormap: full hue cycle
            var hue = t * 360;
            return HSVToRGB(hue, 1.0, 1.0);
        }

        private static Color GetInfernoColor(double t)
        {
            // Inferno colormap approximation
            var colors = new[]
            {
                Color.FromRgb(0, 0, 4),       // Black
                Color.FromRgb(87, 15, 109),   // Dark purple
                Color.FromRgb(180, 54, 122),  // Red purple
                Color.FromRgb(251, 135, 97),  // Orange
                Color.FromRgb(252, 255, 164)  // Light yellow
            };
            return InterpolateColorArray(colors, t);
        }

        private static Color GetJetColor(double t)
        {
            // MATLAB Jet colormap: blue -> cyan -> yellow -> red
            if (t < 0.125) return InterpolateRGB(0, 0, 128, 0, 0, 255, t * 8);
            if (t < 0.375) return InterpolateRGB(0, 0, 255, 0, 255, 255, (t - 0.125) * 4);
            if (t < 0.625) return InterpolateRGB(0, 255, 255, 255, 255, 0, (t - 0.375) * 4);
            if (t < 0.875) return InterpolateRGB(255, 255, 0, 255, 0, 0, (t - 0.625) * 4);
            return InterpolateRGB(255, 0, 0, 128, 0, 0, (t - 0.875) * 8);
        }

        private static Color GetMagmaColor(double t)
        {
            // Magma colormap approximation
            var colors = new[]
            {
                Color.FromRgb(0, 0, 4),       // Black
                Color.FromRgb(87, 16, 110),   // Dark purple
                Color.FromRgb(180, 54, 122),  // Magenta
                Color.FromRgb(251, 136, 97),  // Orange
                Color.FromRgb(252, 253, 191)  // Light yellow
            };
            return InterpolateColorArray(colors, t);
        }

        private static Color GetOrangesColor(double t)
        {
            return InterpolateRGB(255, 245, 235, 127, 39, 4, t);
        }

        private static Color GetPlasmaColor(double t)
        {
            // Plasma colormap approximation
            var colors = new[]
            {
                Color.FromRgb(13, 8, 135),    // Dark blue
                Color.FromRgb(126, 3, 168),   // Purple
                Color.FromRgb(203, 70, 121),  // Magenta
                Color.FromRgb(248, 149, 64),  // Orange
                Color.FromRgb(240, 249, 33)   // Yellow
            };
            return InterpolateColorArray(colors, t);
        }

        private static Color GetPurplesColor(double t)
        {
            return InterpolateRGB(252, 251, 253, 63, 0, 125, t);
        }

        private static Color GetRainbowColor(double t)
        {
            // Classic rainbow: Red -> Orange -> Yellow -> Green -> Blue -> Purple
            return HSVToRGB((1 - t) * 300, 1.0, 1.0);
        }

        private static Color GetRdBuColor(double t)
        {
            // Diverging: Blue -> White -> Red
            if (t < 0.5)
            {
                return InterpolateRGB(33, 102, 172, 255, 255, 255, t * 2);
            }
            return InterpolateRGB(255, 255, 255, 178, 24, 43, (t - 0.5) * 2);
        }

        private static Color GetRedsColor(double t)
        {
            return InterpolateRGB(255, 245, 240, 103, 0, 13, t);
        }

        private static Color GetSpectralColor(double t)
        {
            // Diverging: Red -> Yellow -> Green -> Blue
            var colors = new[]
            {
                Color.FromRgb(213, 62, 79),   // Red
                Color.FromRgb(252, 141, 89),  // Orange
                Color.FromRgb(254, 224, 139), // Yellow
                Color.FromRgb(230, 245, 152), // Light green
                Color.FromRgb(153, 213, 148), // Green
                Color.FromRgb(50, 136, 189)   // Blue
            };
            return InterpolateColorArray(colors, t);
        }

        private static Color GetSpringColor(double t)
        {
            // Magenta to yellow
            var r = 255;
            var g = (byte)(t * 255);
            var b = (byte)(255 - t * 255);
            return Color.FromRgb((byte)r, g, b);
        }

        private static Color GetSummerColor(double t)
        {
            // Green to yellow
            var r = (byte)(t * 255);
            var g = (byte)(128 + t * 127);
            var b = (byte)(102 - t * 102);
            return Color.FromRgb(r, g, b);
        }

        private static Color GetViridisColor(double t)
        {
            // Viridis colormap approximation
            var colors = new[]
            {
                Color.FromRgb(68, 1, 84),     // Dark purple
                Color.FromRgb(59, 82, 139),   // Blue purple
                Color.FromRgb(33, 145, 140),  // Teal
                Color.FromRgb(94, 201, 98),   // Green
                Color.FromRgb(253, 231, 37)   // Yellow
            };
            return InterpolateColorArray(colors, t);
        }

        private static Color GetWinterColor(double t)
        {
            // Blue to cyan
            var r = 0;
            var g = (byte)(t * 255);
            var b = (byte)(255 - t * 128);
            return Color.FromRgb((byte)r, g, b);
        }

        private static Color HSVToRGB(double hue, double saturation, double value)
        {
            var hi = (int)(hue / 60) % 6;
            var f = hue / 60 - Math.Floor(hue / 60);

            var v = (byte)(value * 255);
            var p = (byte)(value * (1 - saturation) * 255);
            var q = (byte)(value * (1 - f * saturation) * 255);
            var t = (byte)(value * (1 - (1 - f) * saturation) * 255);

            return hi switch
            {
                0 => Color.FromRgb(v, t, p),
                1 => Color.FromRgb(q, v, p),
                2 => Color.FromRgb(p, v, t),
                3 => Color.FromRgb(p, q, v),
                4 => Color.FromRgb(t, p, v),
                _ => Color.FromRgb(v, p, q)
            };
        }

        private static Color InterpolateColorArray(Color[] colors, double t)
        {
            if (colors.Length == 0) return Colors.Black;
            if (colors.Length == 1) return colors[0];

            var scaledT = t * (colors.Length - 1);
            var index = (int)Math.Floor(scaledT);
            var fraction = scaledT - index;

            if (index >= colors.Length - 1) return colors[^1];
            if (index < 0) return colors[0];

            var color1 = colors[index];
            var color2 = colors[index + 1];

            return InterpolateRGB(color1.R, color1.G, color1.B, color2.R, color2.G, color2.B, fraction);
        }

        private static Color InterpolateRGB(int r1, int g1, int b1, int r2, int g2, int b2, double t)
        {
            var r = (byte)(r1 + (r2 - r1) * t);
            var g = (byte)(g1 + (g2 - g1) * t);
            var b = (byte)(b1 + (b2 - b1) * t);
            return Color.FromRgb(r, g, b);
        }

        #endregion Methods
    }
}