using System.Globalization;
using AutoOrganize.Library.Extensions;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata.Movie;
using Nito.AsyncEx;
using Nito.Disposables.Internals;
using TMDbLib.Objects.General;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbMetadataProviders;

public partial class ThemoviedbMetadataProvider : IMovieMetadataProvider
{
    public async Task<IEnumerable<MovieMetadata>> SearchMovieAsync(SearchQuery query, string? language = null,
        CancellationToken token = default)
    {
        var searchContainer = await _client.SearchMovieAsync(query.Name, year: query.Year ?? 0,
            language: language, cancellationToken: token).ConfigureAwait(false);
        if (searchContainer?.Results is null)
            return [];

        return (await searchContainer.Results.AsParallel()
            .Select(async x => await GetMovieAsyncInternal(x.Id, language, token).ConfigureAwait(false)).WhenAll().ConfigureAwait(false)).WhereNotNull();
    }

    public async Task<MovieMetadata?> SearchMovieSingleAsync(SearchQuery query, string? language = null,
        CancellationToken token = default)
    {
        var searchContainer = await _client.SearchMovieAsync(query.Name, year: query.Year ?? 0,
            language: language, cancellationToken: token).ConfigureAwait(false);
        if (searchContainer?.Results is not { Count: > 0 })
            return null;

        return await GetMovieAsyncInternal(searchContainer.Results.First().Id, language, token).ConfigureAwait(false);
    }

    public async Task<MovieMetadata?> GetMovieAsync(string id, string? language = null,
        CancellationToken token = default)
    {
        return await GetMovieAsyncInternal(int.Parse(id), language, token).ConfigureAwait(false);
    }

    private async Task<MovieMetadata?> GetMovieAsyncInternal(int id, string? language = null,
        CancellationToken token = default)
    {
        await IfNotHasConfigGet(token).ConfigureAwait(false);
        var movie = await _client.GetMovieAsync(id, language: language, cancellationToken: token).ConfigureAwait(false);
        if (movie is null)
            return null;

        ImagesWithId? images = await _client.GetMovieImagesAsync(movie.Id, token).ConfigureAwait(false);
        return new MovieMetadata
        {
            Name = movie.Title,
            OriginalName = movie.OriginalTitle,
            Runtime = movie.Runtime,
            Revenue = movie.Revenue,
            Backdrops = ImageDataListToGroup(images?.Backdrops),
            Posters = ImageDataListToGroup(images?.Posters),
            Logos = ImageDataListToGroup(images?.Logos),
            Languages = movie.SpokenLanguages?.Where(x => x.Iso_639_1 is not null)
                .Select(x => new CultureInfo(x.Iso_639_1!)).NullIfEmpty()?.ToList(),
            Countries = movie.ProductionCountries?.Where(x => x.Iso_3166_1 is not null)
                .Select(x => new RegionInfo(x.Iso_3166_1!)).NullIfEmpty()?.ToList(),
            Overview = movie.Overview,
            AirDate = movie.ReleaseDate,
            ExternalIds = new Dictionary<string, string>
            {
                [Info.ProviderId] = movie.Id.ToString()
            }
        };
    }
}