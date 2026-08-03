using System.Runtime.CompilerServices;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Movie;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Movie;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Movie;
using AutoOrganize.Library.Services.RequestCoalescers;
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

        ILease? lease;
        do
        {
            (bool acquired, lease) =
                await _flightCoordinator.AcquireAsync(cacheKey, token).ConfigureAwait(false);

            if (acquired)
                break;

            if (!ignoreCache && _cache.TryGetValue(cacheKey, out MovieMetadata? cached) && cached is not null)
                return cached;
        } while (true);

        try
        {
            Movie? movie = await Client.GetMovieAsync(movieId, request.Language,
                extraMethods: MovieMethods.ExternalIds, cancellationToken: token).ConfigureAwait(false);
            if (movie is null) return null;


            ImagesWithId? images = await Client.GetMovieImagesAsync(movieId, request.ImageLanguages, null, token)
                .ConfigureAwait(false);
            var metadata = MovieToMetadata(movie, images);
            _cache.Set(cacheKey, metadata, CacheTime);
            return metadata;
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private async Task<IEnumerable<SearchMovie>> SearchMovieRawAsync(MovieSearchRequest request, bool ignoreCache,
        CancellationToken token)
    {
        string cacheKey = $"search_movie_{request.Name}_{request.Year}_{request.Language}_{request.IncludeAdult}";

        ILease? lease;
        do
        {
            if (!ignoreCache && _cache.TryGetValue(cacheKey, out IEnumerable<SearchMovie>? cached) &&
                cached is not null)
                return cached;

            (bool acquired, lease) =
                await _flightCoordinator.AcquireAsync(cacheKey, token).ConfigureAwait(false);

            if (acquired)
                break;
        } while (true);

        try
        {
            await IfNotHasConfigGet(token).ConfigureAwait(false);
            SearchContainer<SearchMovie>? container = await Client.SearchMovieAsync(request.Name, request.Language,
                    includeAdult: request.IncludeAdult, year: request.Year ?? 0, cancellationToken: token)
                .ConfigureAwait(false);

            var results = container?.Results;
            if (results is { Count: > 0 })
                _cache.Set(cacheKey, results, CacheTime);

            return results ?? [];
        }
        finally
        {
            lease?.Dispose();
        }
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
    private MovieMetadata MovieToMetadata(Movie movie, ImagesWithId? images)
    {
        var metadata = new MovieMetadata
        {
            Name = movie.Title,
            Overview = movie.Overview,
            AirDate = movie.ReleaseDate,
            OriginalName = movie.OriginalTitle,
            Runtime = movie.Runtime,
            Revenue = movie.Revenue,
            Backdrops = ImageDataListToGroup(images?.Backdrops),
            Posters = ImageDataListToGroup(images?.Posters),
            Logos = ImageDataListToGroup(images?.Logos),
            ProviderIds = { [nameof(ProviderType.ThemovieDB)] = movie.Id.ToString() }
        };

        return metadata;
    }
}