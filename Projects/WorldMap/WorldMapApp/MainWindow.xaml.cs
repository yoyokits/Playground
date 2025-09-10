// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapApp
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using LLM.Controls;
    using WorldMapControls.Models;
    using WorldMapControls.Models.Enums;
    using WorldMapControls.Services;

    public partial class MainWindow : Window
    {
        #region Fields

        private readonly Random _rand = new();
        private DependencyPropertyDescriptor? _lastOutputDescriptor;
        private ColorMapType _selectedColorMapType = ColorMapType.Jet;

        #endregion Fields

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            InitializeColorMapCombo();
        }

        #endregion Constructors

        #region Methods

        private static Color HslToRgb(double h, double s, double l)
        {
            // Convert HSL to RGB (0..1)
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double hp = h / 60.0;
            double x = c * (1 - Math.Abs(hp % 2 - 1));

            double r, g, b;
            if (hp < 1)
            {
                (r, g, b) = (c, x, 0);
            }
            else if (hp < 2)
            {
                (r, g, b) = (x, c, 0);
            }
            else if (hp < 3)
            {
                (r, g, b) = (0, c, x);
            }
            else if (hp < 4)
            {
                (r, g, b) = (0, x, c);
            }
            else if (hp < 5)
            {
                (r, g, b) = (x, 0, c);
            }
            else
            {
                (r, g, b) = (c, 0, x);
            }

            double m = l - c / 2;
            byte R = (byte)Math.Round((r + m) * 255);
            byte G = (byte)Math.Round((g + m) * 255);
            byte B = (byte)Math.Round((b + m) * 255);
            return Color.FromRgb(R, G, B);
        }

        private void ApplyColorButton_Click(object sender, RoutedEventArgs e)
        {
            var json = LlmChat.LastOutput;
            if (string.IsNullOrWhiteSpace(json))
            {
                MessageBox.Show("No LLM output to apply.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Use the old property name - the smart parser will handle both formats
            Viewer.CountryColorsJson = json;
        }

        private void ApplyColorMapButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyColorMapToAllCountries(_selectedColorMapType);
        }

        private void ApplyColorMapToAllCountries(ColorMapType colorMapType)
        {
            try
            {
                // Get ALL countries from the enum (ignore MapDictionaries limitations)
                // Use Country enum directly to ensure ALL countries get colored
                var allCountries = Enum.GetValues<Country>()
                    .Where(c => c != Country.Unknown)
                    .OrderBy(c => (int)c) // Sort by enum integer value
                    .ToArray();

                if (allCountries.Length == 0)
                {
                    MessageBox.Show("No countries available to apply colormap.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Create sequential values (0, 1, 2, ..., n-1) for each country
                var values = Enumerable.Range(0, allCountries.Length)
                    .Select(i => (double)i)
                    .ToArray();

                // Map values to colors using the selected colormap
                var colors = ColorMapCalculator.MapValues(values, colorMapType);

                // Create color mappings for ALL countries in the enum
                var colorMappings = new List<CountryColorMapping>();
                for (int i = 0; i < allCountries.Length; i++)
                {
                    var countryEnum = allCountries[i];
                    var color = colors[i];
                    var brush = new SolidColorBrush(color);
                    brush.Freeze(); // Freeze for performance
                    colorMappings.Add(new CountryColorMapping(countryEnum, brush));
                }

                // Apply to map - this will color ALL countries in the enum
                Viewer.CountryColorOverrides = colorMappings;

                // Debug: Show that we're coloring ALL countries by enum ID
                var sample = allCountries.Take(5)
                    .Select(c => $"{c}({(int)c})")
                    .ToArray();

                var debugMessage = $"Applied {colorMapType} colormap to ALL {colorMappings.Count} countries by enum ID.\n" +
                                  $"Sample: {string.Join(", ", sample)}...\n" +
                                  $"Range: {allCountries.First()}({(int)allCountries.First()}) to {allCountries.Last()}({(int)allCountries.Last()})";

                MessageBox.Show(debugMessage, "ColorMap Applied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying colormap: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyRandomPastelCountryColors()
        {
            // Generate a pastel brush per country (excluding Unknown)
            var mappings = Enum.GetValues<Country>()
                .Where(c => c != Country.Unknown)
                .Select(c =>
                {
                    var brush = new SolidColorBrush(GeneratePastelColor());
                    if (brush.CanFreeze) brush.Freeze();
                    return new CountryColorMapping(c, brush);
                })
                .ToList();

            Viewer.CountryColorOverrides = mappings;
        }

        private void ColorMapCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorMapCombo.SelectedItem is string selectedName &&
                Enum.TryParse<ColorMapType>(selectedName, out var colorMapType))
            {
                _selectedColorMapType = colorMapType;
            }
        }

        // Add this method to debug country mapping issues
        private void DiagnoseCountryMappings()
        {
            var allEnumCountries = Enum.GetValues<Country>().Where(c => c != Country.Unknown).ToArray();
            var mappedCountries = MapDictionaries.CountryToName.Keys.Where(c => c != Country.Unknown).ToArray();

            var unmappedEnums = allEnumCountries.Except(mappedCountries).ToArray();
            var extraMapped = mappedCountries.Except(allEnumCountries).ToArray();

            var message = $"Total enum countries: {allEnumCountries.Length}\n" +
                          $"Mapped countries: {mappedCountries.Length}\n" +
                          $"Unmapped enums: {unmappedEnums.Length} - {string.Join(", ", unmappedEnums.Take(5))}\n" +
                          $"Extra mapped: {extraMapped.Length}";

            MessageBox.Show(message, "Country Mapping Diagnosis", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private Color GeneratePastelColor()
        {
            // HSL: hue random, saturation fixed moderate, lightness high
            double h = _rand.NextDouble() * 360.0;
            const double s = 0.45;
            const double l = 0.75;
            return HslToRgb(h, s, l);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Monitor LastOutput changes to enable/disable Apply button
            _lastOutputDescriptor = DependencyPropertyDescriptor.FromProperty(
                LlmChatControl.LastOutputProperty,
                typeof(LlmChatControl));

            _lastOutputDescriptor?.AddValueChanged(LlmChat, (_, _) =>
            {
                ApplyColorButton.IsEnabled = !string.IsNullOrWhiteSpace(LlmChat.LastOutput);
            });
        }

        #endregion Methods
    }
}