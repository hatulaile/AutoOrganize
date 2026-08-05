using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.FileTransferBatchServices;
using AutoOrganize.Library.Services.FileTransferServices;
using AutoOrganize.Library.Services.Metadata;
using AutoOrganize.Library.Services.Metadata.Providers.Abstractions;
using AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbProviders;
using AutoOrganize.Library.Services.NameParsers;
using AutoOrganize.Library.Services.NameParsers.Parsers;
using AutoOrganize.Library.Services.PathNameGenerators;
using AutoOrganize.Library.Services.RequestCoalescers;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize.Library.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAutoOrganizeLibrary()
        {
            services
                .AddSingleton<ParserOptions>()
                .AddSingleton<IFileConfigManager>(_ => new FileConfigManager())
                .AddSingleton<INameParserService, NameParserService>()
                .AddSingleton<IFileTransferService, FileTransferService>()
                .AddSingleton<IProviderService, ProviderService>()
                .AddSingleton<IMetadataService, MetadataService>()
                .AddSingleton<IFlightCoordinator, FlightCoordinator>()
                .AddSingleton<IFileNameGenerator, FileNameGenerator>()
                .AddSingleton<IFileTransferBatchService, FileTransferBatchService>()
                .AddSingleton<INameParserStrategy, TvPathParser>()
                .AddSingleton<INameParserStrategy, MoviePathParser>()
                .AddSingleton<IProvider, ThemoviedbProvider>();

            return services;
        }
    }
}