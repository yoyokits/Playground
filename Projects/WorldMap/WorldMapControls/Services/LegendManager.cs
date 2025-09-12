// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
namespace WorldMapControls.Services
{
    using System;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using WorldMapControls.Models.Enums;

    /// <summary>
    /// Handles legend gradient + tick drawing isolated from control logic.
    /// </summary>
    internal sealed class LegendManager
    {
        private readonly Rectangle _gradientRect;
        private readonly TextBlock _minText;
        private readonly TextBlock _maxText;
        private readonly Canvas _ticksCanvas;

        public LegendManager(Rectangle gradientRect, TextBlock minText, TextBlock maxText, Canvas ticksCanvas)
        { _gradientRect = gradientRect; _minText = minText; _maxText = maxText; _ticksCanvas = ticksCanvas; }

        public static string FormatValue(double v)
        {
            double av = Math.Abs(v); string suffix; double scaled;
            if (av >= 1_000_000_000_000d) { scaled = v / 1_000_000_000_000d; suffix = "T"; }
            else if (av >= 1_000_000_000d) { scaled = v / 1_000_000_000d; suffix = "B"; }
            else if (av >= 1_000_000d) { scaled = v / 1_000_000d; suffix = "M"; }
            else if (av >= 1_000d) { scaled = v / 1_000d; suffix = "K"; }
            else { scaled = v; suffix = string.Empty; }
            return scaled.ToString("0.##") + suffix;
        }

        public void Update(double min, double max, ColorMapType mapType)
        {
            var stops = new GradientStopCollection(); const int steps = 32;
            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                var c = Services.ColorMapCalculator.GetColorAtPosition(t, mapType);
                stops.Add(new GradientStop(c, t));
            }
            _gradientRect.Fill = new LinearGradientBrush(stops, new System.Windows.Point(0, 0.5), new System.Windows.Point(1, 0.5));
            _minText.Text = FormatValue(min);
            _maxText.Text = FormatValue(max);
            DrawTicks();
        }

        private void DrawTicks()
        {
            _ticksCanvas.Children.Clear(); int tickCount = 5;
            double width = _gradientRect.Width <= 0 ? _gradientRect.ActualWidth : _gradientRect.Width;
            for (int i = 0; i < tickCount; i++)
            {
                double frac = (double)i / (tickCount - 1);
                double x = frac * width;
                _ticksCanvas.Children.Add(new Line
                {
                    X1 = x, X2 = x, Y1 = 0, Y2 = 18,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.6,
                    SnapsToDevicePixels = true
                });
            }
        }
    }
}
