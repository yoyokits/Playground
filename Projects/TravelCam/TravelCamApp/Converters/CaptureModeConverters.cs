using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using TravelCamApp.ViewModels;

namespace TravelCamApp.Converters
{
    /// <summary>
    /// Converts CaptureMode to the text shown on the shutter button.
    /// </summary>
    public sealed class CaptureModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is CaptureMode mode ? mode.ToString() : "Photo";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts CaptureMode to button background color.
    /// Photo -> White, Video -> Red.
    /// </summary>
    public sealed class CaptureModeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is CaptureMode.Video ? Colors.Red : Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts CaptureMode to label text color for mode selector.
    /// Active mode -> White, Inactive -> Gray.
    /// Parameter is the mode name to check against: "Photo" or "Video".
    /// </summary>
    public sealed class CaptureModeToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CaptureMode mode && parameter is string targetMode)
            {
                var target = targetMode.Equals("Photo", StringComparison.OrdinalIgnoreCase)
                    ? CaptureMode.Photo : CaptureMode.Video;
                return mode == target ? Colors.White : Colors.Gray;
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts CaptureMode to FontAttributes for mode selector.
    /// Active mode -> Bold, Inactive -> None.
    /// Parameter is the mode name: "Photo" or "Video".
    /// </summary>
    public sealed class CaptureModeToFontAttributesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CaptureMode mode && parameter is string targetMode)
            {
                var target = targetMode.Equals("Photo", StringComparison.OrdinalIgnoreCase)
                    ? CaptureMode.Photo : CaptureMode.Video;
                return mode == target ? FontAttributes.Bold : FontAttributes.None;
            }
            return FontAttributes.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
