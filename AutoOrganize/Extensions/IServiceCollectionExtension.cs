using System;
using System.Linq;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Caches;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.FileTransferBatchServices;
using AutoOrganize.Library.Services.FileTransferServices;
using AutoOrganize.Library.Services.Metadata;
using AutoOrganize.Library.Services.Metadata.Providers;
using AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbMetadataProviders;
using AutoOrganize.Library.Services.NameParsers;
using AutoOrganize.Library.Services.NameParsers.Parsers;
using AutoOrganize.Library.Services.PathNameGenerators;
using AutoOrganize.Library.Services.RequestCoalescers;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.TopLevelServices;
using AutoOrganize.Services.WindowManagers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using ViewModelRegistrationGenerator;
using ConfigJsonSourceGenerationContext = AutoOrganize.Services.ConfigJsonSourceGenerationContext;

namespace AutoOrganize.Extensions;

public static class ServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAutoOrganize()
        {
            services
                .AddSingleton<ParserOptions>()
                .AddSingleton<IFileConfigManager, FileConfigManager>(_ =>
                    new FileConfigManager(contexts: ConfigJsonSourceGenerationContext.Default))
                .AddSingleton(new MemoryCache(new MemoryCacheOptions()));

            services
                .AddSingleton<INavigationService, NavigationService>()
                .AddSingleton<IMetadataCache, MemoryMetadataCache>()
                .AddSingleton<INameParserManager, NameParserManager>()
                .AddSingleton<IStorageServices, StorageServices>()
                .AddSingleton<IFileTransferService, FileTransferService>();

            services
                .AddSingleton<IMetadataManager, MetadataManager>()
                .AddSingleton<IWindowService, WindowService>()
                .AddSingleton<IWindowProvider>(provider =>
                    (IWindowProvider)provider.GetRequiredService<IWindowService>())
                .AddSingleton<IFlightCoordinator, FlightCoordinator>()
                .AddSingleton<IFileNameGenerator, FileNameGenerator>()
                .AddSingleton<IFileTransferBatchService, FileTransferBatchService>()
                .AddSingleton<ILauncherServices, LauncherServices>()
                .AddSingleton<IClipboardServices, ClipboardServices>()
                .AddSingleton<INotificationServices, NotificationServices>();

            services.AddSingleton<ITvParser, TvPathParser>();

            services.AddSingleton<IMovieParser, MoviePathParser>();

            services.AddViewModels()
                .AddMetadataProviders();
            return services;
        }

        public IServiceCollection AddMetadataProviders()
        {
            services
                .AddTransient<IMetadataProvider>(x =>
                    x.GetRequiredKeyedService<IMetadataProvider>(nameof(MetadataProviderType.ThemovieDB)))
                .AddKeyedSingleton<IMetadataProvider, ThemoviedbMetadataProvider>(nameof(MetadataProviderType.ThemovieDB));
            return services;
        }

    }
}