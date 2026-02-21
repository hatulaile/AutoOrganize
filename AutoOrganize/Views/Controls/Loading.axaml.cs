// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

//此处使用了 https://github.com/irihitech/Ursa.Avalonia/blob/main/src/Ursa/Controls/Loading/LoadingIcon.cs 的部分源码
//在 MIT 许可证的允许下进行修改和使用
//完整的许可证 https://github.com/irihitech/Ursa.Avalonia?tab=MIT-1-ov-file
//感谢 Ursa.Avalonia 项目提供的源码参考

using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoOrganize.Views.Controls;

public class Loading : TemplatedControl
{
    public static readonly StyledProperty<bool> IsLoadingProperty = AvaloniaProperty.Register<Loading, bool>(
        nameof(IsLoading));

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
}