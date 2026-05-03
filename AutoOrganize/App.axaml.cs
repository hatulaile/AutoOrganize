using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AutoOrganize.Extensions;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.LoggerServices;
using AutoOrganize.Library.Utils;
using AutoOrganize.ViewLocators;
using AutoOrganize.ViewModels;
using AutoOrganize.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;

namespace AutoOrganize;

public partial class App : Application
{
    [AllowNull]
    public IServiceProvider ServiceProvider;

    public new static App Current => (App)Application.Current!;

    public override void Initialize()
    {
        BuildAppServiceProvider();
        AddViewLocator();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            desktop.MainWindow.DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.Exit += (_, _) =>
            {
                var log = ServiceProvider.GetRequiredService<ILogger<App>>();
                log.LogDebug("应用正在退出，正在执行清理工作...");
                ServiceProvider.GetRequiredService<IFileConfigManager>().SaveAllConfigs();
                ServiceProvider.GetRequiredService<Logger>().Dispose();
                log.LogDebug("应用退出清理完毕");
            };
            desktop.MainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void AddViewLocator()
    {
        DataTemplates.Add(new ViewLocator(ServiceProvider));
    }

    private void BuildAppServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddAutoOrganize();

        services
            .AddSingleton<ILoggerService, LoggerService>()
            .AddSingleton<Logger>(provider =>
            {
                var loggerService = provider.GetRequiredService<ILoggerService>();
                Logger logger = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(loggerService.LevelSwitch)
                    .Filter.ByIncludingOnly(_ => loggerService.IsEnabledLogger)
                    .WriteTo.Conditional
                    (
                        _ => loggerService.IsEnabledLogger,
                        configuration => configuration.Async(sinkConfiguration => sinkConfiguration.File
                        (
                            Path.Join(PathUtils.GetDefaultAppdataPath(), "Logs", "log-.log"),
                            outputTemplate:
                            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{ThreadId}] {Message:lj}{NewLine}{Exception}",
                            fileSizeLimitBytes: 10L * 1024L * 1024L,
                            rollingInterval: RollingInterval.Day
                        ))
                    )
#if DEBUG
                    .WriteTo.Console
                    (
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] [{ThreadId}] {Message:lj}{NewLine}{Exception}"
                    )
#endif
                    .Enrich.WithThreadId()
                    .CreateLogger();

                Log.Logger = logger;
                loggerService.ILogger = logger;
                return logger;
            })
            .AddLogging(builder => builder.AddSerilog());

        services
            .AddSingleton<MainWindow>()
            .AddSingleton<MainWindowViewModel>();
        ServiceProvider = services.BuildServiceProvider();
        ServiceProvider.GetRequiredService<Logger>();
    }
}