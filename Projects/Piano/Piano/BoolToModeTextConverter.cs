namespace Piano;

/// <summary>
/// Converts a boolean IsPlayMode value to display text
/// </summary>
public class BoolToModeTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool isPlayMode)
        {
            return isPlayMode ? "Full Sheet" : "Play Mode";
        }
        return "Toggle Mode";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
