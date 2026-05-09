using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Logging;
using Avalonia.Media;

namespace AutoOrganize.Converters.LogsConverter;

public class LogLevelToFontWeightConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return FontWeight.Normal;

        var level = (LogEventLevel)value;
        if (Enum.IsDefined(level))
        {
            return level switch
            {
                LogEventLevel.Information => FontWeight.Bold,
                LogEventLevel.Warning => FontWeight.Bold,
                LogEventLevel.Error => FontWeight.Bold,
                LogEventLevel.Fatal => FontWeight.Bold,
                _ => FontWeight.Normal
            };
        }
        return FontWeight.Normal;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}