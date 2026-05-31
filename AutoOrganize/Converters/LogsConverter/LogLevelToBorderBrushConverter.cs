using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Logging;
using Avalonia.Media;

namespace AutoOrganize.Converters.LogsConverter;

public class LogLevelToBorderBrushConverter : IValueConverter
{
    private static readonly IBrush VrbBorder = new SolidColorBrush(Color.Parse("#6B7280"));
    private static readonly IBrush DbgBorder = new SolidColorBrush(Color.Parse("#2B7AD4"));
    private static readonly IBrush InfoBorder = new SolidColorBrush(Color.Parse("#2B7AD4"));
    private static readonly IBrush WarnBorder = new SolidColorBrush(Color.Parse("#D97706"));
    private static readonly IBrush ErrorBorder = new SolidColorBrush(Color.Parse("#DC2626"));
    private static readonly IBrush FatalBorder = new SolidColorBrush(Color.Parse("#B91C1C"));
    private static readonly IBrush DefaultBorder = new SolidColorBrush(Color.Parse("#6B7280"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return DefaultBorder;

        var level = (LogEventLevel)value;
        if (Enum.IsDefined(level))
        {
            return level switch
            {
                LogEventLevel.Verbose => VrbBorder,
                LogEventLevel.Debug => DbgBorder,
                LogEventLevel.Information => InfoBorder,
                LogEventLevel.Warning => WarnBorder,
                LogEventLevel.Error => ErrorBorder,
                LogEventLevel.Fatal => FatalBorder,
                _ => DefaultBorder
            };
        }

        return DefaultBorder;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}