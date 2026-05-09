using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace AutoOrganize.Converters.LogsConverter;

public class LogEventToStringConverter : IValueConverter
{
    private MessageTemplateTextFormatter _textFormatter;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LogEvent logEvent) return null;
        using var writer = new StringWriter();
        _textFormatter.Format(logEvent, writer);
        return writer.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public LogEventToStringConverter(string format)
    {
        SetTextFormatter(format);
    }

    [MemberNotNull(nameof(_textFormatter))]
    public virtual void SetTextFormatter(string format)
    {
        _textFormatter = new MessageTemplateTextFormatter(format);
    }
}