using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AutoOrganize.Converters;

public class RemoveTransferMessageConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not int successCount || values[1] is not int failedCount)
            return null;

        return (successCount, failedCount) switch
        {
            (> 0, > 0) => $"将移除成功文件 {successCount} 个，失败文件 {failedCount} 个，是否继续？",
            (> 0, _) => $"将移除成功文件 {successCount} 个，是否继续？",
            (_, > 0) => $"将移除失败文件 {failedCount} 个，是否继续？",
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}