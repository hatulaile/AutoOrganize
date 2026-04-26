using System;
using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Extensions;
using AutoOrganize.ViewModels;
using AutoOrganize.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize;

public partial class App : Application
{
    [AllowNull]
    public IServiceProvider ServiceProvider;

    public new static App Current => (App)Application.Current!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            services.AddAutoOrganize();
            services.AddSingleton<MainWindow>()
                .AddSingleton<MainWindowViewModel>();
            ServiceProvider = services.BuildServiceProvider();

            desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            desktop.MainWindow.DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }
}