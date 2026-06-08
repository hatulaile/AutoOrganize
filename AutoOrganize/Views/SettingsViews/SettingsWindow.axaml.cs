using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AutoOrganize.Views.SettingsViews;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}