using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AutoOrganize.Views.AboutViews;

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