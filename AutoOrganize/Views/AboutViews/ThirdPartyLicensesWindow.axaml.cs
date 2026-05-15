using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoOrganize.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AutoOrganize.Views.AboutViews;

public partial class ThirdPartyLicensesWindow : Window
{
    public static IReadOnlyList<ThirdPartyLicenseModel> ThirdPartyModels => _thirdPartyModels;

    private static List<ThirdPartyLicenseModel> _thirdPartyModels =
    [
        new(".NET", "https://github.com/dotnet", "10.0.7", "MIT",
            "https://github.com/dotnet/runtime?tab=MIT-1-ov-file"),

        new("Avalonia", "https://github.com/AvaloniaUI/Avalonia", "12.0.2", "MIT",
            "https://github.com/AvaloniaUI/Avalonia?tab=MIT-1-ov-file"),

        new("AsyncImageLoader.Avalonia", "https://github.com/AvaloniaUtils/AsyncImageLoader.Avalonia", "3.8.0", "MIT",
            "https://github.com/AvaloniaUtils/AsyncImageLoader.Avalonia?tab=MIT-1-ov-file"),

        new("CommunityToolkit.Mvvm", "https://github.com/CommunityToolkit/dotnet", "8.4.2", "MIT",
            "https://github.com/CommunityToolkit/dotnet?tab=License-1-ov-file"),

        new("HotAvalonia", "https://github.com/Kira-NT/HotAvalonia", "3.1.0", "MIT",
            "https://github.com/Kira-NT/HotAvalonia?tab=MIT-1-ov-file"),

        new("Svg.Skia", "https://github.com/wieslawsoltes/Svg.Skia", "12.0.0.5", "MIT",
            "https://github.com/wieslawsoltes/Svg.Skia?tab=MIT-1-ov-file"),

        new("ProDataGrid", "https://github.com/wieslawsoltes/ProDataGrid", "12.0.0", "MIT",
            "https://github.com/wieslawsoltes/ProDataGrid?tab=MIT-1-ov-file"),

        new("StaticViewLocator", "https://github.com/wieslawsoltes/StaticViewLocator", "0.4.0", "MIT",
            "https://github.com/wieslawsoltes/StaticViewLocator?tab=MIT-1-ov-file"),

        new("IconPacks.Avalonia", "https://github.com/MahApps/IconPacks.Avalonia", "Unknow", "MIT",
            "https://github.com/MahApps/IconPacks.Avalonia?tab=MIT-1-ov-file"),

        new("moq", "https://github.com/moq/moq", "4.20.72", "BSD-3-Clause",
            "https://github.com/devlooped/moq?tab=License-1-ov-file"),

        new("AsyncEx", "https://github.com/StephenCleary/AsyncEx", "5.1.2", "MIT",
            "https://github.com/StephenCleary/AsyncEx?tab=MIT-1-ov-file"),

        new("PolySharp", "https://github.com/Sergio0694/PolySharp/", "1.15.0", "MIT",
            "https://github.com/Sergio0694/PolySharp/?tab=MIT-1-ov-file"),

        new("TMDbLib", "https://github.com/jellyfin/TMDbLib", "3.0.0", "MIT",
            "https://github.com/jellyfin/TMDbLib?tab=MIT-1-ov-file"),

        new("Xaml.Behaviors", "https://github.com/wieslawsoltes/Xaml.Behaviors", "12.0.0", "MIT",
            "https://github.com/wieslawsoltes/Xaml.Behaviors?tab=MIT-1-ov-file"),

        new("xunit", "https://github.com/xunit/xunit", "12.0.0", "Apache-2.0",
            "https://github.com/xunit/xunit?tab=License-1-ov-file"),
    ];

    public ThirdPartyLicensesWindow()
    {
        InitializeComponent();
    }

    private async void HomeButtonOnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await OpenUrl(((ThirdPartyLicenseModel)((Button)e.Source!).DataContext!).HomeUrl);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
        }
    }

    private async void LicenseButtonOnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await OpenUrl(((ThirdPartyLicenseModel)((Button)e.Source!).DataContext!).LicenseUrl);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
        }
    }

    private async Task OpenUrl(string url)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel is null)
            return;
        await topLevel.Launcher.LaunchUriAsync(new Uri(url));
    }
}