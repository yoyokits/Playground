using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using TravelCamApp.ViewModels;

namespace TravelCamApp.Converters
{
    /// <summary>
    /// Converts CaptureMode to the inner shutter circle color.
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
    /// Parameter is the mode name: "Photo" or "Video".
    /// </summary>
    public sealed class CaptureModeToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CaptureMode mode && parameter is string targetMode)
            {
                var target = targetMode.Equals("Photo", StringComparison.OrdinalIgnoreCase)
                    ? CaptureMode.Photo : CaptureMode.Video;
                return mode == target ? Colors.White : Color.FromArgb("#88FFFFFF");
            }
            return Color.FromArgb("#88FFFFFF");
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

    /// <summary>
    /// Converts CaptureMode match to bool for mode indicator dot visibility.
    /// Returns true if the current mode matches the parameter.
    /// </summary>
    public sealed class CaptureModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CaptureMode mode && parameter is string targetMode)
            {
                var target = targetMode.Equals("Photo", StringComparison.OrdinalIgnoreCase)
                    ? CaptureMode.Photo : CaptureMode.Video;
                return mode == target;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts IsFlashOn (bool) to icon color.
    /// On  -> Yellow (#FFD700) so the bolt stands out.
    /// Off -> Gray (#707070) so the white slash line is clearly visible on top.
    /// </summary>
    public sealed class FlashIconColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isOn && isOn
                ? Color.FromArgb("#FFD700")
                : Color.FromArgb("#707070");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts SelectedZoomPresetIndex (int) + parameter index to background color.
    /// Selected -> semi-transparent white, Unselected -> transparent.
    /// </summary>
    public sealed class ZoomPresetBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selected && parameter is string p && int.TryParse(p, out int index))
                return selected == index ? Color.FromArgb("#55FFFFFF") : Colors.Transparent;
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts SelectedZoomPresetIndex (int) + parameter index to text color.
    /// Selected -> bright white, Unselected -> dimmed white.
    /// </summary>
    public sealed class ZoomPresetTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selected && parameter is string p && int.TryParse(p, out int index))
                return selected == index ? Colors.White : Color.FromArgb("#AAFFFFFF");
            return Color.FromArgb("#AAFFFFFF");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Returns the background color for an aspect-ratio segment button.
    /// Active → accent blue, Inactive → dark card.
    /// Parameter is the AspectRatioOption name string.
    /// </summary>
    public sealed class AspectRatioToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AspectRatioOption current && parameter is string p &&
                System.Enum.TryParse<AspectRatioOption>(p, out var target))
                return current == target ? Color.FromArgb("#0A84FF") : Color.FromArgb("#2C2C2E");
            return Color.FromArgb("#2C2C2E");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the text color for an aspect-ratio segment button.
    /// Active → White, Inactive → medium gray.
    /// </summary>
    public sealed class AspectRatioToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AspectRatioOption current && parameter is string p &&
                System.Enum.TryParse<AspectRatioOption>(p, out var target))
                return current == target ? Colors.White : Color.FromArgb("#88FFFFFF");
            return Color.FromArgb("#88FFFFFF");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}