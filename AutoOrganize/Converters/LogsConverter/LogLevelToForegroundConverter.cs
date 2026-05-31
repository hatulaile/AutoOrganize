using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Logging;
using Avalonia.Media;

namespace AutoOrganize.Converters.LogsConverter;

public class LogLevelToForegroundConverter : IValueConverter
{
    private static readonly IBrush VrbColor = new SolidColorBrush(Color.Parse("#9CA3AF"));
    private static readonly IBrush DbgColor = new SolidColorBrush(Color.Parse("#4B9EFF"));
    private static readonly IBrush InfoColor = new SolidColorBrush(Color.Parse("#4B9EFF"));
    private static readonly IBrush WarnColor = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush ErrorColor = new SolidColorBrush(Color.Parse("#E85C5C"));
    private static readonly IBrush FatalColor = new SolidColorBrush(Color.Parse("#FF4444"));
    private static readonly IBrush DefaultColor = new SolidColorBrush(Color.Parse("#D4D4D4"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return DefaultColor;

        var level = (LogEventLevel)value;
        if (Enum.IsDefined(level))
        {
            return level switch
            {
                LogEventLevel.Verbose => VrbColor,
                LogEventLevel.Debug => DbgColor,
                LogEventLevel.Information => InfoColor,
                LogEventLevel.Warning => WarnColor,
                LogEventLevel.Error => ErrorColor,
                LogEventLevel.Fatal => FatalColor,
                _ => DefaultColor
            };
        }

        return DefaultColor;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}