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
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using WorldMapControls.Models;
    using WorldMapControls.Models.Enums;
    using WorldMapControls.Rendering;
    using WorldMapControls.Services;
    using WorldMapControls.Extensions;
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

        private ColorMapType _selectedColorMapType = ColorMapType.Jet;

        #endregion Fields

        #region Dependency Properties

        public static readonly DependencyProperty CountryCodeColorsJsonProperty =
            DependencyProperty.Register(
                nameof(CountryCodeColorsJson),
                typeof(string),
                typeof(WorldMapViewer),
                new PropertyMetadata(null, OnCountryCodeColorsJsonChanged));

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

        public string? CountryCodeColorsJson
        {
            get => (string?)GetValue(CountryCodeColorsJsonProperty);
            set => SetValue(CountryCodeColorsJsonProperty, value);
        }

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

        private static void OnCountryCodeColorsJsonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WorldMapViewer v)
                v.ApplyCountryColorsJson(e.NewValue as string);
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

            // Initialize ColorMap ComboBox
            InitializeColorMapCombo();
        }

        #endregion Constructors

        #region Methods

        private static CountryEnum MapToEnum(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return CountryEnum.Unknown;

            // Log ALL country names from GeoJSON for debugging
            System.Diagnostics.Debug.WriteLine($"[GeoJSON Country] Processing: '{name}'");

            // First try the specialized GeoJSON name mapper for shortened names
            var mappedCountry = GeoJsonNameMapper.MapGeoJsonNameToCountry(name);
            if (mappedCountry != Country.Unknown)
            {
                System.Diagnostics.Debug.WriteLine($"[GeoJSON Mapped] '{name}' -> {mappedCountry}");
                return (CountryEnum)mappedCountry; // Cast to CountryEnum alias
            }

            // Fallback to the existing normalized name lookup
            var normalized = new string(name.Where(ch => char.IsLetterOrDigit(ch)).ToArray()).ToLowerInvariant();
            
            if (MapDictionaries.NormalizedNameToCountry.TryGetValue(normalized, out var country))
            {
                System.Diagnostics.Debug.WriteLine($"[Dictionary Mapped] '{name}' (normalized: '{normalized}') -> {country}");
                return (CountryEnum)country; // Cast to CountryEnum alias
            }

            // Log unmatched names for debugging - this is critical for finding missing mappings
            System.Diagnostics.Debug.WriteLine($"❌ [UNMAPPED COUNTRY] '{name}' (normalized: '{normalized}') -> NOT FOUND");
            return CountryEnum.Unknown;
        }

        private void ApplyColorMapButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyColorMapToCountries(_selectedColorMapType);
        }

        private void ApplyColorMapToCountries(ColorMapType colorMapType)
        {
            try
            {
                var mappings = CountryCodeColorService.BuildColorMappings(colorMapType);
                if (mappings.Count == 0)
                {
                    StatusText.Text = "No country codes available to apply colormap.";
                    return;
                }

                CountryColorOverrides = mappings;
                StatusText.Text = $"Applied {colorMapType} colormap using {mappings.Count} country codes.";
            }
            catch (System.Exception ex)
            {
                StatusText.Text = $"Error applying colormap: {ex.Message}";
            }
        }

        private void ApplyCountryColorOverrides()
        {
            if (_mapRenderer?.PathStyler == null) return;

            var dict = new Dictionary<string, Brush>(StringComparer.OrdinalIgnoreCase);
            if (CountryColorOverrides != null)
            {
                foreach (var mapping in CountryColorOverrides)
                {
                    // Strategy 1: Use MapDictionaries mapping if available
                    if (MapDictionaries.CountryToName.TryGetValue(mapping.Country, out var display))
                    {
                        dict[display] = mapping.Fill;
                    }

                    // Strategy 2: Add common GeoJSON name variations for the country
                    var countryVariations = mapping.Country.GetGeoJsonNameVariations();
                    foreach (var variation in countryVariations)
                    {
                        dict[variation] = mapping.Fill;
                    }

                    // Strategy 3: Use enum name as fallback
                    dict[mapping.Country.ToString()] = mapping.Fill;
                }
            }

            _mapRenderer.PathStyler.OverrideFillResolver = name =>
                dict.TryGetValue(name, out var brush) ? brush : null;

            _mapRenderer.PathStyler.RefreshOverrides();
        }

        private void ApplyCountryColorsJson(string? json)
        {
            // Delegate JSON parsing to the dedicated service
            var result = CountryColorJsonParser.Parse(json);

            CountryColorOverrides = result.ColorMappings.Length > 0 ? result.ColorMappings : null;

            if (StatusText != null)
                StatusText.Text = result.Message;
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

        private void ColorMapCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorMapCombo.SelectedItem is string selectedName &&
                Enum.TryParse<ColorMapType>(selectedName, out var colorMapType))
            {
                _selectedColorMapType = colorMapType;
                StatusText.Text = $"ColorMap changed to {selectedName}";
            }
        }

        private void InitializeColorMapCombo()
        {
            var colorMapTypes = Enum.GetValues<ColorMapType>();
            foreach (var colorMap in colorMapTypes)
            {
                ColorMapCombo.Items.Add(colorMap.ToString());
            }
            ColorMapCombo.SelectedIndex = 0; // Default to Jet
        }

        #if DEBUG
        private void RunMappingDiagnostics()
        {
            // Run final verification first
            FinalMappingVerification.RunFinalVerification();
            
            // Run the comprehensive CountryCode investigation
            CountryCodeMappingInvestigator.RunDebugDiagnostics();
            
            // Run the comprehensive test suite
            var testReport = CountryMappingTestSuite.GenerateTestReport();
            System.Diagnostics.Debug.WriteLine("=== COMPREHENSIVE MAPPING TEST RESULTS ===");
            System.Diagnostics.Debug.WriteLine(testReport);
            
            var report = CountryMappingValidator.GenerateValidationReport();
            System.Diagnostics.Debug.WriteLine("=== COUNTRY MAPPING DIAGNOSTICS ===");
            System.Diagnostics.Debug.WriteLine(report);
            
            // Test the GeoJSON name mapper with common problematic names INCLUDING the exact "Dem. Rep. Congo"
            var testNames = new[]
            {
                "Congo", "Congo, Rep.", "Congo, Dem. Rep.", 
                "Dem. Rep. Congo", "Democratic Rep. Congo", // CRITICAL TEST CASES
                "Central African Rep.", "S. Sudan", "Sudan",
                "Georgia", "Armenia", "Turkmenistan", "Somalia",
                "Iceland", "Ireland", "Lesotho", "Eswatini", "Swaziland",
                "Benin", "Togo", "Djibouti", "Eritrea", "Comoros",
                "Seychelles", "Mauritius", "Brunei", "Bahamas", "Barbados",
                "North Macedonia", "Macedonia", "FYROM", "Czech Republic", "Czechia",
                "Bosnia and Herzegovina", "Vatican City", "Holy See"
            };
            
            System.Diagnostics.Debug.WriteLine("=== GEOJSON NAME MAPPING TEST ===");
            foreach (var testName in testNames)
            {
                var mapped = GeoJsonNameMapper.MapGeoJsonNameToCountry(testName);
                var status = mapped != Country.Unknown ? "✅" : "❌";
                System.Diagnostics.Debug.WriteLine($"{status} '{testName}' -> {mapped}");
            }
            
            // Test the normalized lookup as well
            System.Diagnostics.Debug.WriteLine("=== NORMALIZED NAME LOOKUP TEST ===");
            foreach (var testName in testNames)
            {
                var normalized = new string(testName.Where(ch => char.IsLetterOrDigit(ch)).ToArray()).ToLowerInvariant();
                var found = MapDictionaries.NormalizedNameToCountry.TryGetValue(normalized, out var country);
                var status = found ? "✅" : "❌";
                System.Diagnostics.Debug.WriteLine($"{status} '{testName}' (normalized: '{normalized}') -> {(found ? country : "NOT FOUND")}");
            }

            // Test CountryCode to Country mappings
            System.Diagnostics.Debug.WriteLine("=== COUNTRYCODE TO COUNTRY MAPPING TEST ===");
            var problematicCodes = new[]
            {
                CountryCode.CD, CountryCode.CG, CountryCode.CF, CountryCode.SS, CountryCode.SD,
                CountryCode.GE, CountryCode.AM, CountryCode.TM, CountryCode.SO, CountryCode.IS,
                CountryCode.IE, CountryCode.LS, CountryCode.SZ, CountryCode.BJ, CountryCode.TG,
                CountryCode.DJ, CountryCode.ER, CountryCode.KM, CountryCode.SC, CountryCode.MU,
                CountryCode.BN, CountryCode.BS, CountryCode.BB, CountryCode.MK, CountryCode.BA,
                CountryCode.CZ, CountryCode.VA
            };

            foreach (var code in problematicCodes)
            {
                var country = code.ToCountry();
                var status = country != Country.Unknown ? "✅" : "❌";
                System.Diagnostics.Debug.WriteLine($"{status} {code} -> {country}");
            }
        }
        #endif

        private async Task LoadAndRenderMapAsync()
        {
            try
            {
                StatusText.Text = "Loading world map data...";
                var geoJson = await _resourceLoader.LoadGeoJsonAsync();
                
                #if DEBUG
                // CRITICAL: Extract and log ALL country names from the actual GeoJSON
                System.Diagnostics.Debug.WriteLine("=== ANALYZING ACTUAL GEOJSON COUNTRY NAMES ===");
                var geoJsonReport = GeoJsonCountryExtractor.GenerateCountryNamesReport(geoJson);
                System.Diagnostics.Debug.WriteLine(geoJsonReport);
                #endif
                
                var mapData = ParseGeoJsonData(geoJson);
                _mapRenderer.RenderMap(mapData, MAP_WIDTH, MAP_HEIGHT);
                ApplyCountryColorOverrides();
                StatusText.Text = $"Loaded {mapData.Countries.Count} countries. Hover for details.";
                
                #if DEBUG
                // Run comprehensive mapping diagnostics
                RunMappingDiagnostics();
                
                // Generate fix verification report
                var fixReport = CountryMappingVerifier.GenerateFixReport();
                System.Diagnostics.Debug.WriteLine(fixReport);
                
                // Also run the detailed coverage diagnostics
                var detailed = CountryCodeColorService.DiagnoseDetailed(mapData);
                System.Diagnostics.Debug.WriteLine($"[Coverage] Countries in enum: {detailed.EnumCountries.Count}");
                System.Diagnostics.Debug.WriteLine($"[Coverage] Countries in map: {detailed.MappedCountries.Count}");
                System.Diagnostics.Debug.WriteLine($"[Coverage] Unknown feature names: {detailed.UnknownFeatureNames.Count}");
                
                if (detailed.UnknownFeatureNames.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("=== UNMAPPED COUNTRIES FROM PARSED DATA ===");
                    foreach (var name in detailed.UnknownFeatureNames)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ UNMAPPED: '{name}'");
                    }
                }
                #endif
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

            // Try multiple property names that might contain the country name
            var name =
                properties?["NAME"]?.ToString() ??
                properties?["Name"]?.ToString() ??
                properties?["name"]?.ToString() ??
                properties?["ADMIN"]?.ToString() ??
                properties?["NAME_EN"]?.ToString() ??
                properties?["NAME_LONG"]?.ToString() ??
                properties?["COUNTRY"]?.ToString() ??
                properties?["ADM0_A3"]?.ToString() ??
                "Unknown";

            var enumValue = MapToEnum(name);

            // Debug output to see what countries are being parsed
            if (enumValue == CountryEnum.Unknown && name != "Unknown")
            {
                System.Diagnostics.Debug.WriteLine($"[MapParsing] Unmapped country: '{name}'");
            }

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