using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AutoOrganize.Converters;

public class DateTimeToYearConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime dateTime ? dateTime.Year.ToString(culture) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
