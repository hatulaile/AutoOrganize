using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Logging;
using Avalonia.Media;

namespace AutoOrganize.Converters.LogsConverter;

public class LogLevelToForegroundConverter : IValueConverter
{
    private static readonly IBrush VrbColor = new SolidColorBrush(Color.Parse("#A0A0A0"));
    private static readonly IBrush DbgColor = new SolidColorBrush(Color.Parse("#7DC4E4"));
    private static readonly IBrush InfoColor = new SolidColorBrush(Color.Parse("#7CCF9C"));
    private static readonly IBrush WarnColor = new SolidColorBrush(Color.Parse("#E8D47C"));
    private static readonly IBrush ErrorColor = new SolidColorBrush(Color.Parse("#E88C7C"));
    private static readonly IBrush FatalColor = new SolidColorBrush(Color.Parse("#F0A0A0"));
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