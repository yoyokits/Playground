// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Rendering
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using WpfPath = System.Windows.Shapes.Path;

    /// <summary>
    /// Applies interactive styling and hover highlighting to country paths.
    /// </summary>
    public class PathStyler
    {
        #region Fields

        private static readonly Brush DefaultFill = new SolidColorBrush(Color.FromRgb(245, 245, 245));
        private static readonly Brush DefaultStroke = new SolidColorBrush(Color.FromRgb(140, 140, 140));
        private static readonly Brush HoverFill = new SolidColorBrush(Color.FromRgb(207, 227, 255));
        private static readonly Brush HoverStroke = new SolidColorBrush(Color.FromRgb(51, 102, 204));
        private readonly Dictionary<string, List<WpfPath>> _countryPaths = new(StringComparer.OrdinalIgnoreCase);
        private readonly TextBlock _statusText;
        private string? _currentHoveredCountry;

        #endregion Fields

        #region Constructors

        public PathStyler(TextBlock statusText) => _statusText = statusText;

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Optional resolver to provide per-country override fill brushes.
        /// Return null to use default styling.
        /// </summary>
        public Func<string, Brush?>? OverrideFillResolver { get; set; }

        #endregion Properties

        #region Methods

        public void ApplyStyle(WpfPath path, string countryName)
        {
            if (!_countryPaths.TryGetValue(countryName, out var list))
            {
                list = [];
                _countryPaths[countryName] = list;
            }
            list.Add(path);

            var overrideFill = OverrideFillResolver?.Invoke(countryName);
            path.Fill = overrideFill ?? DefaultFill;
            path.Stroke = DefaultStroke;
            path.StrokeThickness = 0.5;
            path.Cursor = Cursors.Hand;
            path.Tag = countryName;

            RenderOptions.SetEdgeMode(path, EdgeMode.Unspecified);
            RenderOptions.SetBitmapScalingMode(path, BitmapScalingMode.HighQuality);
            path.SnapsToDevicePixels = false;
            path.UseLayoutRounding = false;

            path.MouseEnter += (_, _) => HighlightCountry(countryName, true);
            path.MouseLeave += (_, _) => HighlightCountry(countryName, false);
        }

        public void ClearRegistry()
        {
            _countryPaths.Clear();
            _currentHoveredCountry = null;
        }

        /// <summary>
        /// Re-applies override fills (used when overrides update).
        /// </summary>
        public void RefreshOverrides()
        {
            foreach (var (country, paths) in _countryPaths)
            {
                var overrideFill = OverrideFillResolver?.Invoke(country);
                if (overrideFill != null)
                {
                    foreach (var p in paths)
                        if (p != null) p.Fill = overrideFill;
                }
                else
                {
                    foreach (var p in paths)
                        if (p != null) p.Fill = DefaultFill;
                }
            }
        }

        private void HighlightCountry(string countryName, bool highlight)
        {
            if (!_countryPaths.TryGetValue(countryName, out var paths)) return;

            if (highlight)
            {
                foreach (var path in paths)
                {
                    path.Fill = HoverFill;
                    path.Stroke = HoverStroke;
                    path.StrokeThickness = 1.0;
                }
                _statusText.Text = $"Country: {countryName}";
                _currentHoveredCountry = countryName;
            }
            else
            {
                // Restore override or default
                var overrideFill = OverrideFillResolver?.Invoke(countryName);
                foreach (var path in paths)
                {
                    path.Fill = overrideFill ?? DefaultFill;
                    path.Stroke = DefaultStroke;
                    path.StrokeThickness = 0.5;
                }
                _statusText.Text = "Ready - Hover over countries to see details";
                _currentHoveredCountry = null;
            }
        }

        #endregion Methods
    }
}