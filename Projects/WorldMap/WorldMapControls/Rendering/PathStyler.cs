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
        private const double BaseStrokeThickness = 0.5; // logical stroke thickness at zoom=1
        private readonly Dictionary<string, List<WpfPath>> _countryPaths = new(StringComparer.OrdinalIgnoreCase);
        private readonly TextBlock _statusText;
        private string? _currentHoveredCountry;
        private double _outlineMultiplier = 1.0; // user adjustable factor (1.0 = original)
        private Brush _customOutlineColor = DefaultStroke; // user-selectable outline color
        private Brush _customDefaultFill = DefaultFill; // user-selectable default fill color

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

        /// <summary>
        /// Optional resolver to provide per-country numeric values for tooltips.
        /// Return null if no value is available.
        /// </summary>
        public Func<string, double?>? NumericValueResolver { get; set; }

        /// <summary>
        /// Optional formatter used for numeric value display (default 0.## pattern).
        /// </summary>
        public Func<double, string>? NumericFormatter { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Sets outline thickness multiplier (e.g. 0..5). Values &lt;=0 will map to 0 (no stroke).
        /// </summary>
        public void SetOutlineMultiplier(double multiplier)
        {
            if (multiplier < 0) multiplier = 0;
            if (Math.Abs(_outlineMultiplier - multiplier) < 0.0001) return;
            _outlineMultiplier = multiplier;
        }

        /// <summary>
        /// Sets the custom outline color for all country paths.
        /// </summary>
        public void SetOutlineColor(Color color)
        {
            _customOutlineColor = new SolidColorBrush(color);
            _customOutlineColor.Freeze();
            
            // Update all existing paths with new color (except currently hovered)
            foreach (var (countryName, paths) in _countryPaths)
            {
                if (countryName == _currentHoveredCountry) continue; // don't change hovered country
                
                foreach (var path in paths)
                {
                    path.Stroke = _customOutlineColor;
                }
            }
        }

        /// <summary>
        /// Gets the current custom outline color.
        /// </summary>
        public Color GetOutlineColor()
        {
            return _customOutlineColor is SolidColorBrush scb ? scb.Color : DefaultStroke is SolidColorBrush defaultScb ? defaultScb.Color : Colors.Gray;
        }

        /// <summary>
        /// Sets the custom default fill color for countries without specific overrides.
        /// </summary>
        public void SetDefaultFillColor(Color color)
        {
            _customDefaultFill = new SolidColorBrush(color);
            _customDefaultFill.Freeze();
            
            // Update all existing paths that don't have overrides (except currently hovered)
            foreach (var (countryName, paths) in _countryPaths)
            {
                if (countryName == _currentHoveredCountry) continue; // don't change hovered country
                
                var overrideFill = OverrideFillResolver?.Invoke(countryName);
                if (overrideFill == null) // only update paths without specific overrides
                {
                    foreach (var path in paths)
                    {
                        path.Fill = _customDefaultFill;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current custom default fill color.
        /// </summary>
        public Color GetDefaultFillColor()
        {
            return _customDefaultFill is SolidColorBrush scb ? scb.Color : DefaultFill is SolidColorBrush defaultScb ? defaultScb.Color : Color.FromRgb(245, 245, 245);
        }

        public void ApplyStyle(WpfPath path, string countryName)
        {
            if (!_countryPaths.TryGetValue(countryName, out var list))
            {
                list = [];
                _countryPaths[countryName] = list;
            }
            list.Add(path);

            var overrideFill = OverrideFillResolver?.Invoke(countryName);
            path.Fill = overrideFill ?? _customDefaultFill; // use custom default fill color
            path.Stroke = _customOutlineColor; // use custom outline color
            path.StrokeThickness = BaseStrokeThickness * _outlineMultiplier; // initial; will be adjusted by zoom controller
            path.Cursor = Cursors.Hand;
            path.Tag = countryName;
            path.ToolTip = BuildToolTip(countryName, null); // initial tooltip

            path.MouseEnter += (_, _) => HighlightCountry(countryName, true);
            path.MouseLeave += (_, _) => HighlightCountry(countryName, false);
        }

        public void ClearRegistry()
        {
            _countryPaths.Clear();
            _currentHoveredCountry = null;
        }

        public void RefreshOverrides()
        {
            foreach (var (country, paths) in _countryPaths)
            {
                var overrideFill = OverrideFillResolver?.Invoke(country);
                var numeric = NumericValueResolver?.Invoke(country);
                foreach (var p in paths)
                {
                    p.Fill = overrideFill ?? _customDefaultFill; // use custom default fill color
                    p.ToolTip = BuildToolTip(country, numeric);
                }
            }
        }

        public void AdjustForZoom(double zoom)
        {
            if (zoom <= 0) zoom = 1;
            // Keep visual thickness constant relative to screen by inversely scaling with zoom, then apply multiplier
            var stroke = (BaseStrokeThickness * _outlineMultiplier) / zoom;
            foreach (var list in _countryPaths.Values)
            {
                foreach (var p in list)
                {
                    p.StrokeThickness = stroke;
                }
            }
        }

        private object BuildToolTip(string countryName, double? numeric)
        {
            if (numeric.HasValue)
            {
                var formatted = NumericFormatter != null ? NumericFormatter(numeric.Value) : numeric.Value.ToString("0.##");
                return $"{countryName}: {formatted}";
            }
            return countryName;
        }

        private void HighlightCountry(string countryName, bool highlight)
        {
            if (!_countryPaths.TryGetValue(countryName, out var paths)) return;

            if (highlight)
            {
                var numeric = NumericValueResolver?.Invoke(countryName);
                var formatted = numeric.HasValue ? (NumericFormatter != null ? NumericFormatter(numeric.Value) : numeric.Value.ToString("0.##")) : null;
                foreach (var path in paths)
                {
                    path.Fill = HoverFill;
                    path.Stroke = HoverStroke;
                }
                _statusText.Text = formatted != null ? $"Country: {countryName}  Value: {formatted}" : $"Country: {countryName}";
                _currentHoveredCountry = countryName;
            }
            else
            {
                var overrideFill = OverrideFillResolver?.Invoke(countryName);
                foreach (var path in paths)
                {
                    path.Fill = overrideFill ?? _customDefaultFill; // restore custom default fill color when not hovering
                    path.Stroke = _customOutlineColor; // restore custom outline color when not hovering
                }
                _statusText.Text = "Ready - Hover over countries to see details";
                _currentHoveredCountry = null;
            }
        }

        #endregion Methods
    }
}