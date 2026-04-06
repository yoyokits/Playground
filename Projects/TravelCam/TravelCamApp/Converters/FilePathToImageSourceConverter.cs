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
    /// This ensures MAUI can load local images correctly on all platforms.
    /// </summary>
    public class FilePathToImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine($"[FilePathToImageSourceConverter] Convert called with value: {value}, type: {value?.GetType().Name}");

            if (value is string filePath && !string.IsNullOrEmpty(filePath))
            {
                bool exists = File.Exists(filePath);
                System.Diagnostics.Debug.WriteLine($"[FilePathToImageSourceConverter] Path: {filePath}, exists: {exists}");

                if (exists)
                {
                    try
                    {
                        // Use FromStream for absolute cache paths (app-private ExternalCacheDir).
                        // FromFile() doesn't reliably handle app cache directories on Android.
                        var source = ImageSource.FromStream(() =>
                            new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                        System.Diagnostics.Debug.WriteLine($"[FilePathToImageSourceConverter] Successfully created ImageSource for {filePath}");
                        return source;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[FilePathToImageSourceConverter] Error loading {filePath}: {ex.Message}");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[FilePathToImageSourceConverter] Value is null or empty");
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Not used
            return null;
        }
    }
}
