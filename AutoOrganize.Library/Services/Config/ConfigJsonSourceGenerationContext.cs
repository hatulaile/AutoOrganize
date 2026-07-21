using System.Text.Json.Serialization;
using AutoOrganize.Library.Services.FileTransferServices;
using AutoOrganize.Library.Services.LoggerServices;
using AutoOrganize.Library.Services.Metadata.Providers;
using AutoOrganize.Library.Services.PathNameGenerators.Configs;
using ThemoviedbProviderConfig = AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbProviders.ThemoviedbProviderConfig;

namespace AutoOrganize.Library.Services.Config;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(FileNameGeneratorConfig))]
[JsonSerializable(typeof(TvFileNameGenerationConfig))]
[JsonSerializable(typeof(MovieFileNameGeneratorConfig))]
[JsonSerializable(typeof(ThemoviedbProviderConfig))]
[JsonSerializable(typeof(FileTransferConfig))]
[JsonSerializable(typeof(LoggerConfig))]
public sealed partial class ConfigJsonSourceGenerationContext : JsonSerializerContext;