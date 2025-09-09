// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapApp
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using LLM.Controls;
    using WorldMapControls.Models;
    using WorldMapControls.Models.Enums;

    public partial class MainWindow : Window
    {
        #region Fields

        private readonly Random _rand = new();
        private DependencyPropertyDescriptor? _lastOutputDescriptor;

        #endregion Fields

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
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

            Viewer.CountryColorsJson = json;
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

        private Color GeneratePastelColor()
        {
            // HSL: hue random, saturation fixed moderate, lightness high
            double h = _rand.NextDouble() * 360.0;
            const double s = 0.45;
            const double l = 0.75;
            return HslToRgb(h, s, l);
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

            ApplyRandomPastelCountryColors();
        }

        #endregion Methods
    }
}