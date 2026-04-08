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
    /// Converts a media file path to an ImageSource for thumbnail display.
    /// - For images (.jpg): loads the image file directly using FromFile (async).
    /// - For videos (.mp4): loads the corresponding _thumb.jpg thumbnail if available,
    ///   otherwise shows a default video icon from Resources/Images.
    /// Uses ImageSource.FromFile to enable efficient async loading and caching.
    /// </summary>
    public class ThumbnailConverter : IValueConverter
    {
        // Default video icon - use the video_play.svg from Resources/Images
        private static readonly string VideoIconPath = "video_play.svg";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string filePath || string.IsNullOrEmpty(filePath))
                return null;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            // Image file: load directly using FromFile for async loading
            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
            {
                if (File.Exists(filePath))
                {
                    return ImageSource.FromFile(filePath);
                }
                return null;
            }

            // Video file (.mp4): try to find the extracted thumbnail
            if (extension == ".mp4")
            {
                var baseDir = Path.GetDirectoryName(filePath);
                var baseName = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrEmpty(baseDir) || string.IsNullOrEmpty(baseName))
                {
                    // Fallback to embedded video icon if we can't determine thumbnail path
                    return VideoIconPath;
                }
                var thumbPath = Path.Combine(baseDir, baseName + "_thumb.jpg");

                if (File.Exists(thumbPath))
                {
                    return ImageSource.FromFile(thumbPath);
                }

                // Fallback to embedded video icon
                return VideoIconPath;
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
