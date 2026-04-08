// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System.Globalization;
using System.IO;
using Microsoft.Maui.Controls;

namespace TravelCamApp.Converters
{
    /// <summary>
    /// Returns true if the file extension of the bound file path matches the expected extension.
    /// ConverterParameter should be the extension (e.g., ".mp4" or "mp4").
    /// </summary>
    public class ExtensionEqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string filePath || parameter is not string expectedExt)
                return false;

            var actualExt = Path.GetExtension(filePath).ToLowerInvariant();
            var targetExt = expectedExt.StartsWith(".") ? expectedExt.ToLowerInvariant() : "." + expectedExt.ToLowerInvariant();

            return actualExt == targetExt;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
