using System.Globalization;

namespace TaskFlow.Maui;

/// <summary>Turns the hex strings exposed by TaskFlow.Core into a Color, or a Brush when the target needs one.</summary>
public sealed class HexColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = value is string hex && !string.IsNullOrWhiteSpace(hex) ? Color.FromArgb(hex) : Colors.Transparent;
        return typeof(Brush).IsAssignableFrom(targetType) ? new SolidColorBrush(color) : color;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
