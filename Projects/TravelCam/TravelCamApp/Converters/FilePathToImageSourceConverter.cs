// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TravelCamApp.Converters
{
    /// <summary>
    /// Converts a file path (string) to an ImageSource using ImageSource.FromFile().
    /// FromFile lets the platform handler optimize decode (e.g. subsampling).
    /// </summary>
    public class FilePathToImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string filePath || string.IsNullOrEmpty(filePath))
                return null;

            if (!File.Exists(filePath))
                return null;

            // FromFile lets the platform handler optimize decode (subsampling).
            // Works for both DCIM/CekliCam (MediaStore) and ExternalCacheDir paths.
            return ImageSource.FromFile(filePath);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Not used
            return null;
        }
    }
}
