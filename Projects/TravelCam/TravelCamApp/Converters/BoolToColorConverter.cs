// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokitos       //
// ========================================== //

using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace TravelCamApp.Converters
{
    /// <summary>
    /// Returns <see cref="TrueColor"/> when the bound bool is true, otherwise <see cref="FalseColor"/>.
    /// Used to highlight the info button when the panel is open.
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        /// <summary>Color returned when the value is <c>true</c>.</summary>
        public string TrueColor { get; set; } = "#0A84FF";

        /// <summary>Color returned when the value is <c>false</c>.</summary>
        public string FalseColor { get; set; } = "#AA000000";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var isTrue = value is bool b && b;
            var hex = isTrue ? TrueColor : FalseColor;
            return Color.FromArgb(hex);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
