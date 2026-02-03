using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TravelCamApp.Converters
{
    public class PositionToOptionsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double position)
            {
                // Convert 0.0-1.0 range to LayoutOptions
                if (position < 0.33)
                {
                    return LayoutOptions.Start; // Left or Top
                }
                else if (position < 0.67)
                {
                    return LayoutOptions.Center; // Center
                }
                else
                {
                    return LayoutOptions.End; // Right or Bottom
                }
            }

            return LayoutOptions.End; // Default to end (right/bottom)
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}