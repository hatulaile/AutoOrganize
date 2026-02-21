// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

//此代码修改自 https://github.com/irihitech/Ursa.Avalonia/blob/main/src/Ursa.Themes.Semi/Converters/BrushToColorConverter.cs
//在 MIT 许可证的允许下进行修改和使用
//完整的许可证 https://github.com/irihitech/Ursa.Avalonia?tab=MIT-1-ov-file

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace AutoOrganize.Converters;

public class BrushToColorConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ISolidColorBrush b)
        {
            return b.Color;
        }

        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public override IValueConverter ProvideValue(IServiceProvider _) => this;
}