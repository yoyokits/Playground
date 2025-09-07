// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapApp.Rendering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using WorldMapApp.Models;
    using WpfPath = System.Windows.Shapes.Path;

    /// <summary>
    /// Handles map rendering operations.
    /// </summary>
    public class MapRenderer
    {
        #region Fields

        /// <summary>
        /// Defines the _canvas
        /// </summary>
        private readonly Canvas _canvas;

        /// <summary>
        /// Defines the _pathStyler
        /// </summary>
        private readonly PathStyler _pathStyler;

        /// <summary>
        /// Defines the _statusText
        /// </summary>
        private readonly TextBlock _statusText;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MapRenderer"/> class.
        /// </summary>
        /// <param name="canvas">The canvas<see cref="Canvas"/>.</param>
        /// <param name="statusText">The statusText<see cref="TextBlock"/>.</param>
        public MapRenderer(Canvas canvas, TextBlock statusText)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _statusText = statusText ?? throw new ArgumentNullException(nameof(statusText));
            _pathStyler = new PathStyler(_statusText);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// The RenderFallbackMap.
        /// </summary>
        public void RenderFallbackMap()
        {
            ClearCanvas();
            _pathStyler.ClearRegistry(); // Clear path registry

            const double width = 800;
            const double height = 400;
            SetupCanvas(width, height);

            var samples = new[]
            {
                new { Name = "Sample USA", X = 50, Y = 100, Width = 200, Height = 100 },
                new { Name = "Sample Europe", X = 300, Y = 80, Width = 120, Height = 80 },
                new { Name = "Sample Asia", X = 450, Y = 60, Width = 250, Height = 140 }
            };

            foreach (var sample in samples)
            {
                var rect = CreateSampleRectangle(sample.Name, sample.X, sample.Y, sample.Width, sample.Height);
                _canvas.Children.Add(rect);
            }

            _statusText.Text = $"FALLBACK: Created {samples.Length} sample regions.";
        }

        /// <summary>
        /// The RenderMap.
        /// </summary>
        /// <param name="mapData">The mapData<see cref="MapData"/>.</param>
        /// <param name="width">The width<see cref="double"/>.</param>
        /// <param name="height">The height<see cref="double"/>.</param>
        public void RenderMap(MapData mapData, double width, double height)
        {
            ClearCanvas();
            _pathStyler.ClearRegistry(); // Clear path registry
            SetupCanvas(width, height);
            AddBackground(width, height);

            var countryBounds = new List<(string name, Rect bounds)>();

            foreach (var country in mapData.Countries)
            {
                var paths = CreateCountryPaths(country, width, height);
                foreach (var path in paths)
                {
                    _pathStyler.ApplyStyle(path, country.Name);
                    _canvas.Children.Add(path);

                    if (path.Data is PathGeometry pathGeo)
                        countryBounds.Add((country.Name, pathGeo.Bounds));
                }
            }

            AddCountryLabels(countryBounds);
            _canvas.UpdateLayout();
        }

        /// <summary>
        /// The AddBackground.
        /// </summary>
        /// <param name="width">The width<see cref="double"/>.</param>
        /// <param name="height">The height<see cref="double"/>.</param>
        private void AddBackground(double width, double height)
        {
            var background = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = new SolidColorBrush(Color.FromRgb(240, 248, 255))
            };
            _canvas.Children.Add(background);
        }

        /// <summary>
        /// The AddCountryLabels.
        /// </summary>
        /// <param name="countryBounds">The countryBounds<see cref="List{(string name, Rect bounds)}"/>.</param>
        private void AddCountryLabels(List<(string name, Rect bounds)> countryBounds)
        {
            var labelCreator = new CountryLabelCreator(_canvas);
            foreach (var (name, bounds) in countryBounds)
            {
                labelCreator.AddLabel(name, bounds);
            }
        }

        /// <summary>
        /// The ClearCanvas.
        /// </summary>
        private void ClearCanvas()
        {
            _canvas.Children.Clear();
        }

        /// <summary>
        /// The CreateCountryPaths.
        /// </summary>
        /// <param name="country">The country<see cref="Country"/>.</param>
        /// <param name="width">The width<see cref="double"/>.</param>
        /// <param name="height">The height<see cref="double"/>.</param>
        /// <returns>The <see cref="IEnumerable{WpfPath}"/>.</returns>
        private IEnumerable<WpfPath> CreateCountryPaths(Country country, double width, double height)
        {
            var pathBuilder = new CountryPathBuilder();
            return country.GeometryType switch
            {
                CountryGeometryType.Polygon => new[] { pathBuilder.BuildPolygonPath(country.Geometry, width, height) }.Where(p => p != null)!,
                CountryGeometryType.MultiPolygon => pathBuilder.BuildMultiPolygonPaths(country.Geometry, width, height),
                _ => Enumerable.Empty<WpfPath>()
            };
        }

        /// <summary>
        /// The CreateSampleRectangle.
        /// </summary>
        /// <param name="name">The name<see cref="string"/>.</param>
        /// <param name="x">The x<see cref="double"/>.</param>
        /// <param name="y">The y<see cref="double"/>.</param>
        /// <param name="width">The width<see cref="double"/>.</param>
        /// <param name="height">The height<see cref="double"/>.</param>
        /// <returns>The <see cref="Rectangle"/>.</returns>
        private Rectangle CreateSampleRectangle(string name, double x, double y, double width, double height)
        {
            var rect = new Rectangle
            {
                Width = width,
                Height = height,
                Tag = name,
                Fill = new SolidColorBrush(Color.FromRgb(255, 200, 200)),
                Stroke = new SolidColorBrush(Color.FromRgb(200, 100, 100)),
                StrokeThickness = 2,
                Cursor = Cursors.Hand
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);

            rect.MouseEnter += (_, _) =>
            {
                rect.Fill = new SolidColorBrush(Color.FromRgb(255, 150, 150));
                _statusText.Text = name;
            };
            rect.MouseLeave += (_, _) =>
            {
                rect.Fill = new SolidColorBrush(Color.FromRgb(255, 200, 200));
                _statusText.Text = "Ready";
            };

            return rect;
        }

        /// <summary>
        /// The SetupCanvas.
        /// </summary>
        /// <param name="width">The width<see cref="double"/>.</param>
        /// <param name="height">The height<see cref="double"/>.</param>
        private void SetupCanvas(double width, double height)
        {
            _canvas.Width = width;
            _canvas.Height = height;

            RenderOptions.SetEdgeMode(_canvas, EdgeMode.Unspecified);
            RenderOptions.SetBitmapScalingMode(_canvas, BitmapScalingMode.HighQuality);
        }

        #endregion Methods
    }
}