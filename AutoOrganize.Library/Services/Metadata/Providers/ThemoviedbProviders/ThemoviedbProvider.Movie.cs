using System.Runtime.CompilerServices;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Movie;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Movie;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Movie;
using Microsoft.Extensions.Caching.Memory;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbProviders;

public sealed partial class ThemoviedbProvider
{
    public async Task<IEnumerable<MovieSearchResult>?> SearchAsync
        (MovieSearchRequest request, bool ignoreCache = false, CancellationToken token = default) =>
        (await SearchMovieRawAsync(request, ignoreCache, token).ConfigureAwait(false)).Select(SearchMovieToResult);

    public async Task<MovieMetadata?> GetMetadataAsync(MovieMetadataRequest request,
        bool ignoreCache = false, CancellationToken token = default)
    {
        int movieId;
        await IfNotHasConfigGet(token).ConfigureAwait(false);
        if (request.ProviderIds?.TryGetValue(nameof(ProviderType.ThemovieDB), out string? id) is true &&
            int.TryParse(id, out int parsedId))
        {
            movieId = parsedId;
        }
        else
        {
            SearchMovie? first = (await SearchMovieRawAsync(new MovieSearchRequest
            {
                Name = request.Title ?? string.Empty,
                Year = request.Year,
                Language = request.Language
            }, false, token).ConfigureAwait(false)).FirstOrDefault();

            if (first is null) return null;
            movieId = first.Id;
        }

        string cacheKey = $"metadata_movie_{movieId}_{request.Language}_{request.ImageLanguages}";
        if (!ignoreCache && _cache.TryGetValue(cacheKey, out MovieMetadata? cached) && cached is not null)
            return cached;

        var methods = MovieMethods.Images | MovieMethods.ExternalIds;
        Movie? movie = await Client.GetMovieAsync(movieId, request.Language, request.ImageLanguages,
            extraMethods: methods, cancellationToken: token).ConfigureAwait(false);
        if (movie is null) return null;

        var metadata = MovieToMetadata(movie);
        _cache.Set(cacheKey, metadata, CacheTime);
        return metadata;
    }

    private async Task<IEnumerable<SearchMovie>> SearchMovieRawAsync(MovieSearchRequest request, bool ignoreCache,
        CancellationToken token)
    {
        string cacheKey = GetSearchCacheKey(request.Name, request.Year, request.Language);
        if (!ignoreCache && _cache.TryGetValue(cacheKey, out IEnumerable<SearchMovie>? cached) &&
            cached is not null)
            return cached;

        await IfNotHasConfigGet(token).ConfigureAwait(false);
        SearchContainer<SearchMovie>? container = await Client.SearchMovieAsync(request.Name, request.Language,
                includeAdult: request.IncludeAdult, year: request.Year ?? 0, cancellationToken: token)
            .ConfigureAwait(false);

        var results = container?.Results;
        if (results is { Count: > 0 })
            _cache.Set(cacheKey, results, CacheTime);

        return results ?? [];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MovieSearchResult SearchMovieToResult(SearchMovie searchMovie) => new()
    {
        Title = searchMovie.Title,
        Adult = searchMovie.Adult,
        OriginalTitle = searchMovie.OriginalTitle,
        ReleaseDate = searchMovie.ReleaseDate,
        ProviderIds = { [nameof(ProviderType.ThemovieDB)] = searchMovie.Id.ToString() }
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MovieMetadata MovieToMetadata(Movie movie)
    {
        var metadata = new MovieMetadata
        {
            Name = movie.Title,
            Overview = movie.Overview,
            AirDate = movie.ReleaseDate,
            OriginalName = movie.OriginalTitle,
            Runtime = movie.Runtime,
            Revenue = movie.Revenue,
            Backdrops = ImageDataListToGroup(movie.Images?.Backdrops),
            Posters = ImageDataListToGroup(movie.Images?.Posters),
            Logos = ImageDataListToGroup(movie.Images?.Logos),
            ProviderIds = { [nameof(ProviderType.ThemovieDB)] = movie.Id.ToString() }
        };

        return metadata;
    }
}
