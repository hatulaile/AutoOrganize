using System;
using System.Globalization;
using AutoOrganize.Library.Models.Metadata;
using Avalonia.Data.Converters;

namespace AutoOrganize.Converters;

public class MetadataTypeToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not MetadataType type)
            return "NONE";

        return type switch
        {
            MetadataType.Tv => "电视剧",
            MetadataType.Movie => "电影",
            _ => "NONE"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}