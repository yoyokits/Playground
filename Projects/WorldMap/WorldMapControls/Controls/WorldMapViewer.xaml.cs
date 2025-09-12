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
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using WorldMapControls.Extensions;
    using WorldMapControls.Models;
    using WorldMapControls.Models.Enums;
    using WorldMapControls.Rendering;
    using WorldMapControls.Services;
    using CountryEnum = WorldMapControls.Models.Enums.Country;

    public partial class WorldMapViewer : Grid
    {
        private const double MAP_HEIGHT = 1200;
        private const double MAP_WIDTH = 2000;
        private const double MAX_ZOOM = 8.0;
        private const double MIN_ZOOM = 0.3;
        private const double ZOOM_STEP = 0.15;

        private readonly MapRenderer _mapRenderer;
        private readonly ResourceLoader _resourceLoader;
        private readonly ZoomState _zoomState = new(0.55, MIN_ZOOM, MAX_ZOOM, ZOOM_STEP);

        private ColorMapType _selectedColorMapType = ColorMapType.Jet;
        private double _legendMin = 0;
        private double _legendMax = 100;
        private bool _hasNumericRange = false;
        private DispatcherTimer? _colorMapDebounceTimer;
        private ColorMapType _pendingColorMapType;
        private IReadOnlyDictionary<CountryEnum, double> _numericCountryValues = new Dictionary<CountryEnum, double>();

        // Panning state
        private Point? _dragOriginViewport;
        private Point? _dragOriginTranslate;

        private LegendManager? _legendManager;

        private double _outlineMultiplier = 1.0;

        // Event to notify host app of outline thickness changes (host persists setting)
        public event EventHandler<double>? OutlineThicknessChanged;

        // Event to notify host app of outline color changes (host persists setting)
        public event EventHandler<Color>? OutlineColorChanged;

        // Event to notify host app of default fill color changes (host persists setting)
        public event EventHandler<Color>? DefaultFillColorChanged;

        #region Dependency Properties
        public static readonly DependencyProperty JsonProperty = DependencyProperty.Register(
            nameof(Json), typeof(string), typeof(WorldMapViewer), new PropertyMetadata(null, OnJsonChanged));

        public static readonly DependencyProperty CountryColorOverridesProperty = DependencyProperty.Register(
            nameof(CountryColorOverrides), typeof(IEnumerable<CountryColorMapping>), typeof(WorldMapViewer), new PropertyMetadata(null, OnCountryColorOverridesChanged));

        public static readonly DependencyProperty ColorMapTypeProperty = DependencyProperty.Register(
            nameof(ColorMapType), typeof(ColorMapType), typeof(WorldMapViewer), new PropertyMetadata(ColorMapType.Jet, OnColorMapTypeChanged));

        public string? Json { get => (string?)GetValue(JsonProperty); set => SetValue(JsonProperty, value); }
        public IEnumerable<CountryColorMapping>? CountryColorOverrides { get => (IEnumerable<CountryColorMapping>?)GetValue(CountryColorOverridesProperty); set => SetValue(CountryColorOverridesProperty, value); }
        public ColorMapType ColorMapType { get => (ColorMapType)GetValue(ColorMapTypeProperty); set => SetValue(ColorMapTypeProperty, value); }

        private static void OnJsonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        { if (d is WorldMapViewer v) v.ApplyRegionJson(e.NewValue as string); }
        private static void OnCountryColorOverridesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        { if (d is WorldMapViewer v) v.ApplyCountryColorOverrides(); }
        private static void OnColorMapTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        { if (d is not WorldMapViewer v) return; var nt = (ColorMapType)e.NewValue; if (v._selectedColorMapType == nt) return; v._selectedColorMapType = nt; if (!string.IsNullOrWhiteSpace(v.Json)) v.ApplyRegionJson(v.Json); else v.UpdateLegendGradient(); }
        #endregion

        public WorldMapViewer()
        {
            InitializeComponent();
            _resourceLoader = new ResourceLoader();
            _mapRenderer = new MapRenderer(MapCanvas, StatusText);
            PopulateColorMapItems();
            AddHandler(MouseWheelEvent, new MouseWheelEventHandler(OnViewerMouseWheel), true);
            MapViewport.PreviewMouseLeftButtonDown += OnViewportMouseDown;
            MapViewport.PreviewMouseLeftButtonUp += OnViewportMouseUp;
            MapViewport.PreviewMouseMove += OnViewportMouseMove;
            _legendManager = new LegendManager(LegendGradient, LegendMinText, LegendMaxText, LegendTicks);
            
            // Initialize color preview when loaded
            Loaded += (s, e) => 
            {
                var defaultOutlineColor = _mapRenderer.PathStyler.GetOutlineColor();
                UpdateOutlineColorPreview(defaultOutlineColor);
                
                var defaultFillColor = _mapRenderer.PathStyler.GetDefaultFillColor();
                UpdateDefaultFillColorPreview(defaultFillColor);
            };
        }

        #region Zoom & Pan
        private void ApplyZoom(double newZoom, Point? pivot = null)
        {
            if (!_zoomState.Set(newZoom)) return;
            if (ScaleTx == null || TranslateTx == null) return;
            var scale = _zoomState.Zoom;
            if (pivot.HasValue)
            {
                var vp = pivot.Value;
                var worldX = (vp.X - TranslateTx.X) / ScaleTx.ScaleX;
                var worldY = (vp.Y - TranslateTx.Y) / ScaleTx.ScaleY;
                ScaleTx.ScaleX = scale; ScaleTx.ScaleY = scale;
                TranslateTx.X = vp.X - worldX * scale;
                TranslateTx.Y = vp.Y - worldY * scale;
            }
            else { ScaleTx.ScaleX = scale; ScaleTx.ScaleY = scale; }
            ZoomText.Text = $"{Math.Round(scale * 100, 0)}%";
            _mapRenderer.PathStyler.AdjustForZoom(scale);
            ClampPan();
        }

        private void ClampPan()
        {
            if (TranslateTx == null || ScaleTx == null) return;
            var scale = _zoomState.Zoom;
            var viewW = MapViewport.ActualWidth;
            var viewH = MapViewport.ActualHeight;
            var contentW = MAP_WIDTH * scale;
            var contentH = MAP_HEIGHT * scale;
            double minX = Math.Min(0, viewW - contentW);
            double maxX = 0;
            double minY = Math.Min(0, viewH - contentH);
            double maxY = 0;
            // Center if content smaller
            if (contentW < viewW) { TranslateTx.X = (viewW - contentW) / 2.0; } else { TranslateTx.X = Math.Clamp(TranslateTx.X, minX, maxX); }
            if (contentH < viewH) { TranslateTx.Y = (viewH - contentH) / 2.0; } else { TranslateTx.Y = Math.Clamp(TranslateTx.Y, minY, maxY); }
        }

        private void CenterIfSmaller() => ClampPan();

        private void OnViewerMouseWheel(object? sender, MouseWheelEventArgs e)
        { ApplyZoom(_zoomState.Zoom + (e.Delta > 0 ? ZOOM_STEP : -ZOOM_STEP), e.GetPosition(MapViewport)); e.Handled = true; }

        private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _dragOriginViewport = e.GetPosition(MapViewport);
                _dragOriginTranslate = new Point(TranslateTx.X, TranslateTx.Y);
                MapViewport.Cursor = System.Windows.Input.Cursors.SizeAll;
                // Capture only the viewport (previously captured parent Grid causing lost Up event)
                MapViewport.CaptureMouse();
            }
        }
        private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragOriginViewport = null;
            _dragOriginTranslate = null;
            MapViewport.Cursor = System.Windows.Input.Cursors.Arrow;
            if (Mouse.Captured == MapViewport)
                Mouse.Capture(null);
        }
        private void OnViewportMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_dragOriginViewport.HasValue && _dragOriginTranslate.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                var cur = e.GetPosition(MapViewport);
                var dx = cur.X - _dragOriginViewport.Value.X;
                var dy = cur.Y - _dragOriginViewport.Value.Y;
                TranslateTx.X = _dragOriginTranslate.Value.X + dx;
                TranslateTx.Y = _dragOriginTranslate.Value.Y + dy;
                ClampPan();
            }
        }
        private void MapViewport_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Mouse.Captured == MapViewport)
            {
                _dragOriginViewport = null;
                _dragOriginTranslate = null;
                MapViewport.Cursor = System.Windows.Input.Cursors.Arrow;
                Mouse.Capture(null);
            }
        }
        private void MapViewport_SizeChanged(object sender, SizeChangedEventArgs e) => CenterIfSmaller();
        #endregion

        #region Legend & Formatting
        private static string FormatLegendValue(double v)
        { double av = Math.Abs(v); string suffix; double scaled; if (av >= 1_000_000_000_000d) { scaled = v / 1_000_000_000_000d; suffix = "T"; } else if (av >= 1_000_000_000d) { scaled = v / 1_000_000_000d; suffix = "B"; } else if (av >= 1_000_000d) { scaled = v / 1_000_000d; suffix = "M"; } else if (av >= 1_000d) { scaled = v / 1_000d; suffix = "K"; } else { scaled = v; suffix = string.Empty; } return scaled.ToString("0.##") + suffix; }

        private void UpdateLegendGradient()
        { _legendManager?.Update(_legendMin, _legendMax, _selectedColorMapType); }
        #endregion

        #region Parsing / JSON / ColorMap
        private void ApplyRegionJson(string? json)
        {
            var extracted = JsonInputExtractor.ExtractJson(json);
            if (extracted == null)
            {
                CountryColorOverrides = null;
                _numericCountryValues = new Dictionary<CountryEnum, double>();
                _legendMin = 0; _legendMax = 100; _hasNumericRange = false;
                StatusText.Text = "No JSON detected.";
                UpdateLegendGradient();
                return;
            }
            var result = CountryColorJsonParser.Parse(extracted, _selectedColorMapType);
            CountryColorOverrides = result.ColorMappings.Length > 0 ? result.ColorMappings : null;
            _numericCountryValues = result.NumericValues.ToDictionary(kv => (CountryEnum)kv.Key, kv => kv.Value);
            if (StatusText != null) StatusText.Text = result.Message;
            var nm = ExtractNumericPairs(extracted);
            if (nm.Count > 0) { _legendMin = nm.Min(kv => kv.Value); _legendMax = nm.Max(kv => kv.Value); _hasNumericRange = true; }
            else { _legendMin = 0; _legendMax = 100; _hasNumericRange = false; }
            UpdateLegendGradient();
            ApplyCountryColorOverrides();
        }

        private Dictionary<string, double> ExtractNumericPairs(string? json)
        { var dict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase); if (string.IsNullOrWhiteSpace(json)) return dict; try { using var doc = JsonDocument.Parse(json); var root = doc.RootElement; if (root.ValueKind == JsonValueKind.Object) foreach (var p in root.EnumerateObject()) { if (p.Value.ValueKind == JsonValueKind.Number && p.Value.TryGetDouble(out var d)) dict[p.Name] = d; else if (p.Value.ValueKind == JsonValueKind.String && double.TryParse(p.Value.GetString(), out var ds)) dict[p.Name] = ds; } } catch { } return dict; }

        private void EnsureColorMapDebounceTimer()
        { if (_colorMapDebounceTimer != null) return; _colorMapDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) }; _colorMapDebounceTimer.Tick += (_, _) => { _colorMapDebounceTimer!.Stop(); if (ColorMapType != _pendingColorMapType) ColorMapType = _pendingColorMapType; else UpdateLegendGradient(); }; }

        private void ColorMapCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorMapCombo.SelectedItem is ColorMapPreview preview && Enum.TryParse<ColorMapType>(preview.Name, out var type))
            {
                _pendingColorMapType = type;
                EnsureColorMapDebounceTimer();
                _colorMapDebounceTimer!.Stop();
                _colorMapDebounceTimer.Start();
                StatusText.Text = $"ColorMap (pending) {preview.Name}";
                PreviewLegend(type);
            }
        }

        private void PreviewLegend(ColorMapType previewType)
        { var rect = LegendGradient; if (rect == null) return; var stops = new GradientStopCollection(); const int steps = 24; for (int i = 0; i <= steps; i++) { double t = (double)i / steps; var c = ColorMapCalculator.GetColorAtPosition(t, previewType); stops.Add(new GradientStop(c, t)); } rect.Fill = new LinearGradientBrush(stops, new Point(0, 0.5), new Point(1, 0.5)); }
        #endregion

        #region Data Loading
        private async Task LoadAndRenderMapAsync()
        { try { StatusText.Text = "Loading world map data..."; var geoJson = await _resourceLoader.LoadGeoJsonAsync(); var mapData = ParseGeoJsonData(geoJson); _mapRenderer.RenderMap(mapData, MAP_WIDTH, MAP_HEIGHT); ApplyCountryColorOverrides(); ApplyZoom(_zoomState.Zoom, new Point(MapViewport.ActualWidth / 2, MapViewport.ActualHeight / 2)); StatusText.Text = $"Loaded {mapData.Countries.Count} countries. Hover for details."; UpdateLegendGradient(); } catch (Exception ex) { StatusText.Text = $"Failed to load map: {ex.Message}"; _mapRenderer.RenderFallbackMap(); } }
        #endregion

        #region GeoJSON Helpers
        private CountryInfo? ParseCountryFeature(JsonNode? feature)
        { if (feature == null) return null; var props = feature["properties"]; var geom = feature["geometry"]; var name = props?["NAME"]?.ToString() ?? props?["Name"]?.ToString() ?? props?["name"]?.ToString() ?? props?["ADMIN"]?.ToString() ?? props?["NAME_EN"]?.ToString() ?? props?["NAME_LONG"]?.ToString() ?? props?["COUNTRY"]?.ToString() ?? props?["ADM0_A3"]?.ToString() ?? "Unknown"; var ev = MapToEnum(name); return geom?["type"]?.ToString() switch { "Polygon" => new CountryInfo(name, ev, CountryGeometryType.Polygon, geom), "MultiPolygon" => new CountryInfo(name, ev, CountryGeometryType.MultiPolygon, geom), _ => null }; }

        private static CountryEnum MapToEnum(string name)
        { if (string.IsNullOrWhiteSpace(name)) return CountryEnum.Unknown; var mapped = GeoJsonNameMapper.MapGeoJsonNameToCountry(name); if (mapped != Country.Unknown) return (CountryEnum)mapped; var norm = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant(); if (MapDictionaries.NormalizedNameToCountry.TryGetValue(norm, out var c)) return (CountryEnum)c; return CountryEnum.Unknown; }

        private MapData ParseGeoJsonData(string json)
        { var root = JsonNode.Parse(json) ?? throw new InvalidOperationException("Invalid GeoJSON format"); var features = root["features"] as JsonArray ?? throw new InvalidOperationException("No features found in GeoJSON"); var list = new List<CountryInfo>(); foreach (var f in features) { var c = ParseCountryFeature(f); if (c != null) list.Add(c); } return new MapData(list); }
        #endregion

        #region Initialization
        private void PopulateColorMapItems()
        {
            if (ColorMapCombo == null) return;
            var list = new List<ColorMapPreview>();
            foreach (var cm in Enum.GetValues<ColorMapType>())
            {
                var stops = new GradientStopCollection();
                const int steps = 24;
                for (int i = 0; i <= steps; i++)
                {
                    double t = (double)i / steps;
                    var c = ColorMapCalculator.GetColorAtPosition(t, cm);
                    stops.Add(new GradientStop(c, t));
                }
                var brush = new LinearGradientBrush(stops, new Point(0, 0.5), new Point(1, 0.5));
                brush.Freeze();
                list.Add(new ColorMapPreview(cm.ToString(), brush));
            }
            ColorMapCombo.ItemsSource = list;
            ColorMapCombo.SelectedIndex = 0;
        }
        private void RefreshColorMapItemGradients()
        {
            for (int i = 0; i < ColorMapCombo.Items.Count; i++)
            {
                var item = ColorMapCombo.ItemContainerGenerator.ContainerFromIndex(i) as ComboBoxItem;
                if (item == null) continue;
                var border = FindDescendant<Border>(item);
                if (border == null) continue;
                if (border.Background is LinearGradientBrush lgb)
                {
                    lgb.GradientStops.Clear();
                    const int steps = 16;
                    if (Enum.TryParse<ColorMapType>(ColorMapCombo.Items[i].ToString(), out var mapType))
                    {
                        for (int s = 0; s <= steps; s++)
                        {
                            double t = (double)s / steps;
                            var c = ColorMapCalculator.GetColorAtPosition(t, mapType);
                            lgb.GradientStops.Add(new GradientStop(c, t));
                        }
                    }
                }
            }
        }
        private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T t) return t;
                var d = FindDescendant<T>(child);
                if (d != null) return d;
            }
            return null;
        }

        private void ApplyCountryColorOverrides()
        { var dict = new Dictionary<string, Brush>(StringComparer.OrdinalIgnoreCase); if (CountryColorOverrides != null) { foreach (var m in CountryColorOverrides) { if (MapDictionaries.CountryToName.TryGetValue(m.Country, out var display)) dict[display] = m.Fill; foreach (var v in m.Country.GetGeoJsonNameVariations()) dict[v] = m.Fill; dict[m.Country.ToString()] = m.Fill; } } _mapRenderer.PathStyler.OverrideFillResolver = name => dict.TryGetValue(name, out var b) ? b : null; _mapRenderer.PathStyler.NumericValueResolver = name => { var norm = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant(); if (MapDictionaries.NormalizedNameToCountry.TryGetValue(norm, out var ctry)) { var ev = (CountryEnum)ctry; if (_numericCountryValues.TryGetValue(ev, out var val)) return val; } return null; }; _mapRenderer.PathStyler.NumericFormatter = v => v.ToString("0.##"); _mapRenderer.PathStyler.RefreshOverrides(); _mapRenderer.PathStyler.AdjustForZoom(_zoomState.Zoom); }

        private async void Window_Loaded(object sender, RoutedEventArgs e) { await LoadAndRenderMapAsync(); }
        #endregion

        internal sealed class ZoomState
        {
            public double Zoom { get; private set; }
            public double MinZoom { get; }
            public double MaxZoom { get; }
            public double Step { get; }
            public ZoomState(double initial, double min, double max, double step)
            { Zoom = initial; MinZoom = min; MaxZoom = max; Step = step; }
            public bool Set(double value)
            { var clamped = Math.Clamp(value, MinZoom, MaxZoom); if (Math.Abs(clamped - Zoom) < 0.0001) return false; Zoom = clamped; return true; }
            public bool Increment(int wheelDelta) => Set(Zoom + (wheelDelta > 0 ? Step : -Step));
        }

        private record ColorMapPreview(string Name, LinearGradientBrush Brush);

        private void OutlineSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            _outlineMultiplier = e.NewValue; // 0-5
            var tb = FindName("OutlineValueText") as TextBlock;
            if (tb != null) tb.Text = _outlineMultiplier.ToString("0.0");
            _mapRenderer?.PathStyler.SetOutlineMultiplier(_outlineMultiplier);
            _mapRenderer?.PathStyler.AdjustForZoom(_zoomState.Zoom); // reapply with current zoom
            OutlineThicknessChanged?.Invoke(this, _outlineMultiplier);
        }

        private void OutlineSlider_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Slider s)
            {
                s.Minimum = 0;
                s.Maximum = 5;
                s.SmallChange = 0.1;
                s.LargeChange = 0.5;
                s.TickFrequency = 0.5;
                s.IsSnapToTickEnabled = false; // Allow continuous movement
                if (s.Value <= 0) s.Value = 1.0; // default
                _outlineMultiplier = s.Value;
                var tb = FindName("OutlineValueText") as TextBlock;
                if (tb != null) tb.Text = _outlineMultiplier.ToString("0.0");
                _mapRenderer?.PathStyler.SetOutlineMultiplier(_outlineMultiplier);
                _mapRenderer?.PathStyler.AdjustForZoom(_zoomState.Zoom);
            }
        }

        private void OutlineColorPicker_Click(object sender, RoutedEventArgs e)
        {
            // Create a WPF color picker dialog
            var currentColor = _mapRenderer?.PathStyler.GetOutlineColor() ?? Colors.Gray;
            var colorDialog = new ColorPickerDialog(currentColor)
            {
                Owner = Window.GetWindow(this),
                Title = "Select Outline Color"
            };
            
            if (colorDialog.ShowDialog() == true)
            {
                var selectedColor = colorDialog.SelectedColor;
                
                // Apply the color
                _mapRenderer?.PathStyler.SetOutlineColor(selectedColor);
                
                // Update the color preview button
                UpdateOutlineColorPreview(selectedColor);
                
                // Notify host app
                OutlineColorChanged?.Invoke(this, selectedColor);
            }
        }

        private void DefaultFillColorPicker_Click(object sender, RoutedEventArgs e)
        {
            // Create a WPF color picker dialog
            var currentColor = _mapRenderer?.PathStyler.GetDefaultFillColor() ?? Color.FromRgb(245, 245, 245);
            var colorDialog = new ColorPickerDialog(currentColor)
            {
                Owner = Window.GetWindow(this),
                Title = "Select Default Fill Color"
            };
            
            if (colorDialog.ShowDialog() == true)
            {
                var selectedColor = colorDialog.SelectedColor;
                
                // Apply the color
                _mapRenderer?.PathStyler.SetDefaultFillColor(selectedColor);
                
                // Update the color preview button
                UpdateDefaultFillColorPreview(selectedColor);
                
                // Notify host app
                DefaultFillColorChanged?.Invoke(this, selectedColor);
            }
        }

        private void UpdateOutlineColorPreview(Color color)
        {
            if (FindName("OutlineColorPicker") is System.Windows.Controls.Button colorButton && 
                colorButton.Template?.FindName("ColorPreview", colorButton) is System.Windows.Shapes.Rectangle colorRect)
            {
                colorRect.Fill = new SolidColorBrush(color);
            }
        }

        private void UpdateDefaultFillColorPreview(Color color)
        {
            if (FindName("DefaultFillColorPicker") is System.Windows.Controls.Button colorButton && 
                colorButton.Template?.FindName("ColorPreview", colorButton) is System.Windows.Shapes.Rectangle colorRect)
            {
                colorRect.Fill = new SolidColorBrush(color);
            }
        }

        /// <summary>
        /// Sets the outline color programmatically (e.g., from saved settings)
        /// </summary>
        public void SetOutlineColor(Color color)
        {
            _mapRenderer?.PathStyler.SetOutlineColor(color);
            UpdateOutlineColorPreview(color);
        }

        /// <summary>
        /// Sets the default fill color programmatically (e.g., from saved settings)
        /// </summary>
        public void SetDefaultFillColor(Color color)
        {
            _mapRenderer?.PathStyler.SetDefaultFillColor(color);
            UpdateDefaultFillColorPreview(color);
        }

        /// <summary>
        /// Gets the current outline thickness value
        /// </summary>
        public double GetOutlineThickness() => _outlineMultiplier;

        /// <summary>
        /// Sets the outline thickness programmatically (e.g., from saved settings)
        /// </summary>
        public void SetOutlineThickness(double thickness)
        {
            if (Math.Abs(_outlineMultiplier - thickness) < 0.001) return;
            
            _outlineMultiplier = thickness;
            _mapRenderer?.PathStyler.SetOutlineMultiplier(_outlineMultiplier);
            _mapRenderer?.PathStyler.AdjustForZoom(_zoomState.Zoom);
            
            // Update slider if it exists
            var slider = FindName("OutlineSlider") as Slider;
            if (slider != null && Math.Abs(slider.Value - thickness) > 0.001)
            {
                slider.Value = thickness;
            }
            
            // Update text display
            var tb = FindName("OutlineValueText") as TextBlock;
            if (tb != null) tb.Text = thickness.ToString("0.0");
        }
    }
}