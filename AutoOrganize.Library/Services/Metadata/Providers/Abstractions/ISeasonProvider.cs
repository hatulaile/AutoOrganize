using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;

namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface ISeasonMetadataProvider : IMetadataProvider<SeasonMetadataRequest, SeasonMetadata>;

public interface ISeasonProvider : ISeasonMetadataProvider;