using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Logging;
using Avalonia.Media;

namespace AutoOrganize.Converters.LogsConverter;

public sealed class LevelToRowBackgroundConverter : IValueConverter
{
    private static readonly IBrush VrbBg = new SolidColorBrush(Color.Parse("#2A2A2A"));
    private static readonly IBrush DbgBg = new SolidColorBrush(Color.Parse("#1A2A3A"));
    private static readonly IBrush InfoBg = new SolidColorBrush(Color.Parse("#1A2A2A"));
    private static readonly IBrush WarnBg = new SolidColorBrush(Color.Parse("#2A2A1A"));
    private static readonly IBrush ErrorBg = new SolidColorBrush(Color.Parse("#2A1A1A"));
    private static readonly IBrush FatalBg = new SolidColorBrush(Color.Parse("#3A1A1A"));
    private static readonly IBrush DefaultBg = Brushes.Transparent;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return DefaultBg;
        var level = (LogEventLevel)value;
        if (Enum.IsDefined(level))
        {
            return level switch
            {
                LogEventLevel.Verbose => VrbBg,
                LogEventLevel.Debug => DbgBg,
                LogEventLevel.Information => InfoBg,
                LogEventLevel.Warning => WarnBg,
                LogEventLevel.Error => ErrorBg,
                LogEventLevel.Fatal => FatalBg,
                _ => DefaultBg
            };
        }

        return DefaultBg;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}