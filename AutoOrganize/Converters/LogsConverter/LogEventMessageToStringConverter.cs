using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace AutoOrganize.Converters.LogsConverter;

public class LogEventMessageToStringConverter : IValueConverter
{
    private static char[] MessageFormat => ['\r', '\n'];

    private readonly MessageTemplateTextFormatter _textFormatter = new("{Message:lj}");

    private readonly MessageTemplateTextFormatter _errorTextFormatter = new("{Message:lj}{NewLine}{Exception}");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LogEvent logEvent) return null;
        using var writer = new StringWriter();
        if (logEvent.Exception is null) _textFormatter.Format(logEvent, writer);
        else _errorTextFormatter.Format(logEvent, writer);
        return writer.ToString().Trim(MessageFormat);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}