using AutoOrganize.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void LicenseClick(object? sender, RoutedEventArgs e)
    {
        var window = new LicenseWindow();
        window.Show(this);
    }

    private void ThirdPartyLicenseClick(object? sender, RoutedEventArgs e)
    {
        var window = new ThirdPartyLicensesWindow();
        window.Show(this);
    }
}