using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AutoOrganize.Converters;

public class RegionInfoToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is RegionInfo region)
            return region.ToString();

        if (value is IEnumerable<RegionInfo> regionInfos)
            return string.Join(", ", regionInfos);

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}