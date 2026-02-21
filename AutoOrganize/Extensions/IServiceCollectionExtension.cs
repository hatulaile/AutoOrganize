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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using TMDbLib.Client;
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
                .AddSingleton<IMetadataCache, MemoryMetadataCache>()
                .AddSingleton<INameParserManager, NameParserManager>()
                .AddSingleton<IStorageServices, StorageServices>()
                .AddSingleton<IFileTransferService, FileTransferService>();

            services
                .AddSingleton<IMetadataManager, MetadataManager>()
                .AddSingleton<IFlightCoordinator, FlightCoordinator>()
                .AddSingleton<IPathNameGenerator, PathNameGenerator>()
                .AddSingleton<IFileTransferBatchService, FileTransferBatchService>()
                .AddSingleton<ILauncherServices, LauncherServices>()
                .AddSingleton<IClipboardServices, ClipboardServices>()
                .AddSingleton<INotificationServices, NotificationServices>();

            services.AddSingleton<ITvParser, TvPathParser>();

            services.AddSingleton<IMovieParser, MoviePathParser>();

            services.AddViewModels()
                .AddNavigationService()
                .AddMetadataProviders();
            return services;
        }

        public IServiceCollection AddNavigationService()
        {
            services
                .AddSingleton<INavigationService, NavigationService>()
                .AddRoutingState(HostScreens.Main)
                .AddRoutingState(HostScreens.Home)
                .AddRoutingState(HostScreens.MetadataEdit);
            return services;
        }

        public IServiceCollection AddMetadataProviders()
        {
            services
                .AddTransient<IMetadataProvider>(x =>
                    x.GetRequiredKeyedService<IMetadataProvider>(nameof(MetadataProviderType.ThemovieDB)))
                .AddKeyedSingleton<IMetadataProvider>(nameof(MetadataProviderType.ThemovieDB),
                    (provider, _) => new ThemoviedbMetadataProvider(
                        //这里的 api key 是我的, 请不要到其他地方使用, 申请自己的 api key 地址 https://www.themoviedb.org/settings/api
                        //这里使用了 api.tmdb.org, 国内访问默认的 api.themoviedb.org 会有问题
                        new TMDbClient("a68ae3528e2875c12cca9e924c5483b5", baseUrl: "api.tmdb.org"),
                        provider.GetRequiredService<IFileConfigManager>()
                            .GetConfigOrLoad<ThemoviedbMetadataProviderConfig>()));

            return services;
        }

        private IServiceCollection AddRoutingState(HostScreens screens)
        {
            services.AddKeyedSingleton<RoutingState>(screens);
            return services;
        }
    }
}