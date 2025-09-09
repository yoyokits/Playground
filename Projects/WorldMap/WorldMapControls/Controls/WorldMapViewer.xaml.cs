// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using WorldMapControls.Models;
    using WorldMapControls.Models.Enums;
    using WorldMapControls.Rendering;
    using WorldMapControls.Services;
    using CountryEnum = WorldMapControls.Models.Enums.Country;

    public partial class WorldMapViewer
    {
        #region Fields

        private const double MAP_HEIGHT = 1200;
        private const double MAP_WIDTH = 2000;
        private const double MAX_ZOOM = 8.0;
        private const double MIN_ZOOM = 0.3;
        private const double ZOOM_STEP = 0.15;

        private readonly MapRenderer _mapRenderer;
        private readonly ResourceLoader _resourceLoader;
        private readonly ZoomController _zoomController;

        #endregion Fields

        #region Constructors

        public WorldMapViewer()
        {
            InitializeComponent();
            _resourceLoader = new ResourceLoader();
            _mapRenderer = new MapRenderer(MapCanvas, StatusText);
            _zoomController = new ZoomController(MIN_ZOOM, MAX_ZOOM, ZOOM_STEP);
        }

        #endregion Constructors

        #region Methods

        private static CountryEnum MapToEnum(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return CountryEnum.Unknown;
            var normalized = Normalize(name);
            return MapDictionaries.NormalizedNameToCountry.TryGetValue(normalized, out var c)
                ? c
                : CountryEnum.Unknown;

            static string Normalize(string value) =>
                new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }

        private void ApplyZoomTransform(ZoomResult zoomResult, Point mousePosition)
        {
            ZoomTransform.ScaleX = zoomResult.NewZoom;
            ZoomTransform.ScaleY = zoomResult.NewZoom;

            var sv = MapScrollViewer;
            var factor = zoomResult.NewZoom / zoomResult.PreviousZoom;

            var newH = mousePosition.X * factor - (mousePosition.X - sv.HorizontalOffset);
            var newV = mousePosition.Y * factor - (mousePosition.Y - sv.VerticalOffset);

            sv.ScrollToHorizontalOffset(newH);
            sv.ScrollToVerticalOffset(newV);
        }

        private void HandleInitializationError(Exception ex)
        {
            var message = $"Initialization error: {ex.Message}";
            MessageBox.Show($"{message}\n\nStack trace: {ex.StackTrace}",
                "Application Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            if (StatusText != null) StatusText.Text = message;
        }

        private async Task LoadAndRenderMapAsync()
        {
            try
            {
                StatusText.Text = "Loading world map data...";
                var geoJson = await _resourceLoader.LoadGeoJsonAsync();
                var mapData = ParseGeoJsonData(geoJson);
                _mapRenderer.RenderMap(mapData, MAP_WIDTH, MAP_HEIGHT);
                StatusText.Text = $"Loaded {mapData.Countries.Count} countries. Hover for details.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Failed to load map: {ex.Message}";
                _mapRenderer.RenderFallbackMap();
            }
        }

        private void MapScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                var pos = e.GetPosition(MapCanvas);
                var result = _zoomController.HandleZoom(e.Delta, pos);
                if (result.ZoomChanged)
                {
                    ApplyZoomTransform(result, pos);
                    UpdateZoomDisplay(result.NewZoom);
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Zoom error: {ex.Message}";
            }
        }

        private CountryInfo? ParseCountryFeature(JsonNode? feature)
        {
            if (feature == null) return null;

            var properties = feature["properties"];
            var geometry = feature["geometry"];

            var name =
                properties?["NAME"]?.ToString()
                ?? properties?["Name"]?.ToString()
                ?? properties?["name"]?.ToString()
                ?? properties?["ADMIN"]?.ToString()
                ?? "Unknown";

            var enumValue = MapToEnum(name);

            return geometry?["type"]?.ToString() switch
            {
                "Polygon" => new CountryInfo(name, enumValue, CountryGeometryType.Polygon, geometry),
                "MultiPolygon" => new CountryInfo(name, enumValue, CountryGeometryType.MultiPolygon, geometry),
                _ => null
            };
        }

        private MapData ParseGeoJsonData(string json)
        {
            var root = JsonNode.Parse(json) ?? throw new InvalidOperationException("Invalid GeoJSON format");
            var features = root["features"] as JsonArray ?? throw new InvalidOperationException("No features found in GeoJSON");

            var countries = new List<CountryInfo>();
            foreach (var feature in features)
            {
                var c = ParseCountryFeature(feature);
                if (c != null) countries.Add(c);
            }
            return new MapData(countries);
        }

        private void UpdateZoomDisplay(double zoom) =>
            ZoomText.Text = $"{Math.Round(zoom * 100, 0, MidpointRounding.AwayFromZero)}%";

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadAndRenderMapAsync();
            }
            catch (Exception ex)
            {
                HandleInitializationError(ex);
            }
        }

        #endregion Methods
    }
}