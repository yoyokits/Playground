// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Rendering
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using WpfPath = System.Windows.Shapes.Path;

    /// <summary>
    /// Handles path styling and hover effects with country-wide highlighting.
    /// </summary>
    public class PathStyler
    {
        #region Fields

        private readonly Dictionary<string, List<WpfPath>> _countryPaths = new();
        private readonly TextBlock _statusText;
        private string? _currentHoveredCountry;

        #endregion Fields

        #region Constructors

        public PathStyler(TextBlock statusText) => _statusText = statusText;

        #endregion Constructors

        #region Methods

        public void ApplyStyle(WpfPath path, string countryName)
        {
            if (!_countryPaths.ContainsKey(countryName))
                _countryPaths[countryName] = new List<WpfPath>();

            _countryPaths[countryName].Add(path);

            path.Fill = new SolidColorBrush(Color.FromRgb(245, 245, 245));
            path.Stroke = new SolidColorBrush(Color.FromRgb(140, 140, 140));
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

        private void HighlightCountry(string countryName, bool highlight)
        {
            if (highlight && _currentHoveredCountry == countryName) return;
            if (!highlight && _currentHoveredCountry != countryName) return;
            if (!_countryPaths.TryGetValue(countryName, out var paths)) return;

            if (highlight)
            {
                foreach (var path in paths)
                {
                    path.Fill = new SolidColorBrush(Color.FromRgb(207, 227, 255));
                    path.Stroke = new SolidColorBrush(Color.FromRgb(51, 102, 204));
                    path.StrokeThickness = 1.0;
                }
                _statusText.Text = $"Country: {countryName}";
                _currentHoveredCountry = countryName;
            }
            else
            {
                foreach (var path in paths)
                {
                    path.Fill = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                    path.Stroke = new SolidColorBrush(Color.FromRgb(140, 140, 140));
                    path.StrokeThickness = 0.5;
                }
                _statusText.Text = "Ready - Hover over countries to see details";
                _currentHoveredCountry = null;
            }
        }

        #endregion Methods
    }
}