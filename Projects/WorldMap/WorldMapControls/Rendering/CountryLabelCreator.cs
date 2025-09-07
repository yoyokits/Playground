// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapApp.Rendering
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Creates country labels.
    /// </summary>
    public class CountryLabelCreator
    {
        #region Fields

        private readonly Canvas _canvas;

        #endregion Fields

        #region Constructors

        public CountryLabelCreator(Canvas canvas) => _canvas = canvas;

        #endregion Constructors

        #region Methods

        public void AddLabel(string countryName, Rect bounds)
        {
            if (bounds.Width < 80 || bounds.Height < 40) return;
            var label = CreateLabel(countryName, bounds);
            PositionLabel(label, countryName, bounds);
            _canvas.Children.Add(label);
        }

        private TextBlock CreateLabel(string countryName, Rect bounds)
        {
            var label = new TextBlock
            {
                Text = countryName,
                Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                FontWeight = FontWeights.Medium,
                FontSize = Math.Max(11, Math.Min(bounds.Width / 10, 16)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
                UseLayoutRounding = false,
                SnapsToDevicePixels = false
            };

            RenderOptions.SetClearTypeHint(label, ClearTypeHint.Enabled);
            TextOptions.SetTextRenderingMode(label, TextRenderingMode.ClearType);
            TextOptions.SetTextFormattingMode(label, TextFormattingMode.Display);

            label.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.White,
                BlurRadius = 1,
                ShadowDepth = 0.5,
                Opacity = 0.9,
                Direction = 0
            };

            return label;
        }

        private void PositionLabel(TextBlock label, string countryName, Rect bounds)
        {
            var centerX = bounds.X + bounds.Width / 2;
            var centerY = bounds.Y + bounds.Height / 2;

            var formattedText = new FormattedText(
                countryName,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch),
                label.FontSize,
                label.Foreground,
                VisualTreeHelper.GetDpi(_canvas).PixelsPerDip);

            Canvas.SetLeft(label, centerX - formattedText.Width / 2);
            Canvas.SetTop(label, centerY - formattedText.Height / 2);
        }

        #endregion Methods
    }
}