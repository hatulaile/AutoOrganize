using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Tv;

namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface ISeriesSearchProvider : ISearchProvider<SeriesSearchRequest, SeriesSearchResult>;

public interface ISeriesMetadataProvider : IMetadataProvider<SeriesMetadataRequest, SeriesMetadata>;

public interface ISeriesProvider : ISeriesSearchProvider, ISeriesMetadataProvider;