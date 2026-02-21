using System.Text.Json.Serialization;
using AutoOrganize.Library.Services.FileTransferServices;
using AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbMetadataProviders;
using AutoOrganize.Library.Services.PathNameGenerators.Configs;

namespace AutoOrganize.Library.Services.Config;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(FileNameGeneratorConfig))]
[JsonSerializable(typeof(TvFileNameGenerationConfig))]
[JsonSerializable(typeof(MovieFileNameGeneratorConfig))]
[JsonSerializable(typeof(ThemoviedbMetadataProviderConfig))]
[JsonSerializable(typeof(FileTransferConfig))]
public sealed partial class ConfigJsonSourceGenerationContext : JsonSerializerContext;