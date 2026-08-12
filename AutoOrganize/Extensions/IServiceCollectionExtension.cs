using System;
using AutoOrganize.Library.Extensions;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.TopLevelServices;
using AutoOrganize.Services.WindowManagers;
using Microsoft.Extensions.DependencyInjection;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.Extensions;

public static class ServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAutoOrganize()
        {
            services.AddAutoOrganizeLibrary();

            services
                .AddSingleton<INavigationService, NavigationService>()
                .AddSingleton<IStorageServices, StorageServices>()
                .AddSingleton<IWindowService, WindowService>()
                .AddSingleton<IWindowProvider>(provider =>
                    (IWindowProvider)provider.GetRequiredService<IWindowService>())
                .AddSingleton<ILauncherServices, LauncherServices>()
                .AddSingleton<IClipboardServices, ClipboardServices>()
                .AddSingleton<INotificationServices, NotificationServices>()
                .AddSingleton<IToastServices, ToastServices>();

            services.AddViewModels();
            return services;
        }
    }
}