// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;          // Added for Grid base
    using System.Windows.Input;
    using System.Windows.Media;
    using WorldMapControls.Models;
    using WorldMapControls.Models.Enums;
    using WorldMapControls.Rendering;
    using WorldMapControls.Services;
    using CountryEnum = WorldMapControls.Models.Enums.Country;

    // IMPORTANT: Must inherit from Grid because XAML root element is <Grid>
    public partial class WorldMapViewer : Grid
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

        #region Dependency Properties

        public static readonly DependencyProperty CountryColorOverridesProperty =
            DependencyProperty.Register(
                nameof(CountryColorOverrides),
                typeof(IEnumerable<CountryColorMapping>),
                typeof(WorldMapViewer),
                new PropertyMetadata(null, OnCountryColorOverridesChanged));

        public static readonly DependencyProperty CountryColorsJsonProperty =
            DependencyProperty.Register(
                nameof(CountryColorsJson),
                typeof(string),
                typeof(WorldMapViewer),
                new PropertyMetadata(null, OnCountryColorsJsonChanged));

        public IEnumerable<CountryColorMapping>? CountryColorOverrides
        {
            get => (IEnumerable<CountryColorMapping>?)GetValue(CountryColorOverridesProperty);
            set => SetValue(CountryColorOverridesProperty, value);
        }

        public string? CountryColorsJson
        {
            get => (string?)GetValue(CountryColorsJsonProperty);
            set => SetValue(CountryColorsJsonProperty, value);
        }

        private static void OnCountryColorOverridesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WorldMapViewer viewer) viewer.ApplyCountryColorOverrides();
        }

        private static void OnCountryColorsJsonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WorldMapViewer v)
                v.ApplyCountryColorsJson(e.NewValue as string);
        }

        #endregion Dependency Properties

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
            var normalized = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
            return MapDictionaries.NormalizedNameToCountry.TryGetValue(normalized, out var c) ? c : CountryEnum.Unknown;
        }

        private void ApplyCountryColorOverrides()
        {
            if (_mapRenderer?.PathStyler == null) return;

            var dict = new Dictionary<string, Brush>(StringComparer.OrdinalIgnoreCase);
            if (CountryColorOverrides != null)
            {
                foreach (var mapping in CountryColorOverrides)
                {
                    if (MapDictionaries.CountryToName.TryGetValue(mapping.Country, out var display))
                        dict[display] = mapping.Fill;
                    else
                        dict[mapping.Country.ToString()] = mapping.Fill;
                }
            }

            _mapRenderer.PathStyler.OverrideFillResolver = name =>
                dict.TryGetValue(name, out var brush) ? brush : null;

            _mapRenderer.PathStyler.RefreshOverrides();
        }

        private void ApplyCountryColorsJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                CountryColorOverrides = null;
                if (StatusText != null) StatusText.Text = "Color overrides cleared.";
                return;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var list = new List<CountryColorMapping>();

                void TryAdd(string? countryName, string? colorStr)
                {
                    if (string.IsNullOrWhiteSpace(countryName) || string.IsNullOrWhiteSpace(colorStr))
                        return;

                    // Normalize color
                    if (!colorStr.StartsWith("#", StringComparison.Ordinal))
                    {
                        // Named color attempt
                        try
                        {
                            var named = (Color)ColorConverter.ConvertFromString(colorStr);
                            colorStr = $"#{named.A:X2}{named.R:X2}{named.G:X2}{named.B:X2}";
                        }
                        catch
                        {
                            return;
                        }
                    }

                    // Map to enum
                    var normalized = new string(countryName.ToLowerInvariant()
                        .Replace("'", "")
                        .Replace("-", "")
                        .Replace(" ", "")
                        .ToCharArray());

                    if (!MapDictionaries.NormalizedNameToCountry.TryGetValue(normalized, out var enumCountry))
                        return;

                    // Create brush
                    Brush fill;
                    try
                    {
                        var c = (Color)ColorConverter.ConvertFromString(colorStr);
                        fill = new SolidColorBrush(c);
                        fill.Freeze();
                    }
                    catch
                    {
                        return;
                    }

                    // Positional record usage (fixes: no parameterless ctor for CountryColorMapping)
                    list.Add(new CountryColorMapping(enumCountry, fill));
                }

                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in root.EnumerateObject())
                    {
                        var colorVal = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : null;
                        TryAdd(prop.Name, colorVal);
                    }
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in root.EnumerateArray())
                    {
                        switch (el.ValueKind)
                        {
                            case JsonValueKind.Object:
                                {
                                    string? country = null;
                                    string? color = null;
                                    if (el.TryGetProperty("country", out var cEl) && cEl.ValueKind == JsonValueKind.String)
                                        country = cEl.GetString();
                                    if (el.TryGetProperty("color", out var colEl) && colEl.ValueKind == JsonValueKind.String)
                                        color = colEl.GetString();
                                    else if (el.TryGetProperty("value", out var vCol) && vCol.ValueKind == JsonValueKind.String)
                                        color = vCol.GetString();
                                    TryAdd(country, color);
                                }
                                break;

                            case JsonValueKind.Array:
                                if (el.GetArrayLength() == 2 &&
                                    el[0].ValueKind == JsonValueKind.String &&
                                    el[1].ValueKind == JsonValueKind.String)
                                {
                                    TryAdd(el[0].GetString(), el[1].GetString());
                                }
                                break;
                        }
                    }
                }

                CountryColorOverrides = list.Count > 0 ? list : null;
                if (StatusText != null)
                    StatusText.Text = list.Count > 0
                        ? $"Applied {list.Count} color overrides."
                        : "No valid country colors found.";
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                    StatusText.Text = $"Invalid color JSON: {ex.Message}";
            }
        }

        private void ApplyZoomTransform(ZoomResult zr, Point mouse)
        {
            ZoomTransform.ScaleX = zr.NewZoom;
            ZoomTransform.ScaleY = zr.NewZoom;

            var sv = MapScrollViewer;
            var factor = zr.NewZoom / zr.PreviousZoom;

            var newH = mouse.X * factor - (mouse.X - sv.HorizontalOffset);
            var newV = mouse.Y * factor - (mouse.Y - sv.VerticalOffset);

            sv.ScrollToHorizontalOffset(newH);
            sv.ScrollToVerticalOffset(newV);
        }

        private async Task LoadAndRenderMapAsync()
        {
            try
            {
                StatusText.Text = "Loading world map data...";
                var geoJson = await _resourceLoader.LoadGeoJsonAsync();
                var mapData = ParseGeoJsonData(geoJson);
                _mapRenderer.RenderMap(mapData, MAP_WIDTH, MAP_HEIGHT);
                ApplyCountryColorOverrides();
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
                properties?["NAME"]?.ToString() ??
                properties?["Name"]?.ToString() ??
                properties?["name"]?.ToString() ??
                properties?["ADMIN"]?.ToString() ??
                "Unknown";

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
            try { await LoadAndRenderMapAsync(); }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion Methods
    }
}