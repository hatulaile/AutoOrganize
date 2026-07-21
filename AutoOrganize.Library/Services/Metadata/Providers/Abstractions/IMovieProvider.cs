using AutoOrganize.Library.Services.Metadata.Models.Metadata.Movie;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Movie;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Movie;

namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface IMovieSearchProvider : ISearchProvider<MovieSearchRequest, MovieSearchResult>;

public interface IMovieMetadataProvider : IMetadataProvider<MovieMetadataRequest, MovieMetadata>;

public interface IMovieProvider : IMovieSearchProvider, IMovieMetadataProvider;