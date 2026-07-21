using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;

namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface IEpisodeMetadataProvider : IMetadataProvider<EpisodeMetadataRequest, EpisodeMetadata>;

public interface IEpisodeProvider : IEpisodeMetadataProvider;