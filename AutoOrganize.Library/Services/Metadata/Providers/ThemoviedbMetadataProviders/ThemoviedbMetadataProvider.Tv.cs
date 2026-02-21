using System.Globalization;
using AutoOrganize.Library.Extensions;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata.Tv;
using Nito.AsyncEx;
using Nito.Disposables.Internals;
using TMDbLib.Objects.TvShows;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbMetadataProviders;

public partial class ThemoviedbMetadataProvider : ITvMetadataProvider
{
    public async Task<IEnumerable<SeriesMetadata>> SearchSeriesAsync(SearchQuery query, string? language = null,
        CancellationToken token = default)
    {
        var searchContainer = await _client.SearchTvShowAsync(query.Name, firstAirDateYear: query.Year ?? 0,
            language: language, cancellationToken: token).ConfigureAwait(false);
        if (searchContainer?.Results is null)
            return [];

        await IfNotHasConfigGet(token).ConfigureAwait(false);
        return (await searchContainer.Results.AsParallel()
            .Select(async x => await GetSeriesAsyncInternal(x.Id, language, token).ConfigureAwait(false)).WhenAll().ConfigureAwait(false)).WhereNotNull();
    }

    public async Task<SeriesMetadata?> SearchSeriesSingleAsync(SearchQuery query, string? language = null,
        CancellationToken token = default)
    {
        var searchContainer = await _client.SearchTvShowAsync(query.Name, firstAirDateYear: query.Year ?? 0,
            language: language, cancellationToken: token).ConfigureAwait(false);
        if (searchContainer?.Results is not { Count: > 0 })
            return null;

        return await GetSeriesAsyncInternal(searchContainer.Results.First().Id, language, token).ConfigureAwait(false);
    }

    public async Task<SeriesMetadata?> GetSeriesAsync(string id, string? language = null,
        CancellationToken token = default)
    {
        return await GetSeriesAsyncInternal(int.Parse(id), language, token).ConfigureAwait(false);
    }

    public async Task<SeasonMetadata?> GetSeasonMetadataAsync(string id, int seasonNumber, string? language = null,
        CancellationToken token = default)
    {
        await IfNotHasConfigGet(token).ConfigureAwait(false);
        TvSeason? season =
            await _client.GetTvSeasonAsync(int.Parse(id), seasonNumber, language: language, cancellationToken: token).ConfigureAwait(false);
        if (season is null)
            return null;

        return new SeasonMetadata
        {
            Name = season.Name,
            Overview = season.Overview,
            AirDate = season.AirDate,
            Posters = ImageDataListToGroup(season.Images?.Posters),
            SeasonNumber = seasonNumber
        };
    }

    public async Task<EpisodeMetadata?> GetEpisodeMetadataAsync(string id, int seasonNumber, long episodeNumber,
        string? language = null, CancellationToken token = default)
    {
        await IfNotHasConfigGet(token).ConfigureAwait(false);
        TvEpisode? episode =
            await _client.GetTvEpisodeAsync(int.Parse(id), seasonNumber, episodeNumber, language: language,
                cancellationToken: token).ConfigureAwait(false);
        if (episode is null)
            return null;

        return new EpisodeMetadata
        {
            Name = episode.Name,
            Overview = episode.Overview,
            AirDate = episode.AirDate,
            // 这里的 Stills 就是 Backdrops. season/0/episode/1/images/backdrops
            Backdrops = ImageDataListToGroup(episode.Images?.Stills),
            EpisodeNumber = episodeNumber
        };
    }

    private async Task<SeriesMetadata?> GetSeriesAsyncInternal(int id, string? language = null,
        CancellationToken token = default)
    {
        await IfNotHasConfigGet(token).ConfigureAwait(false);
        var tv = await _client.GetTvShowAsync(id, language: language,
            cancellationToken: token).ConfigureAwait(false);
        if (tv is null) return null;

        var images = await _client.GetTvShowImagesAsync(tv.Id, cancellationToken: token).ConfigureAwait(false);

        return new SeriesMetadata
        {
            Name = tv.Name,
            OriginalName = tv.OriginalName,
            InProduction = tv.InProduction,
            Languages = tv.SpokenLanguages?.Where(x => x.Iso_639_1 is not null)
                .Select(x => new CultureInfo(x.Iso_639_1!)).NullIfEmpty()?.ToList(),
            Countries = tv.ProductionCountries?.Where(x => x.Iso_3166_1 is not null)
                .Select(x => new RegionInfo(x.Iso_3166_1!)).NullIfEmpty()?.ToList(),
            Backdrops = ImageDataListToGroup(images?.Backdrops),
            Posters = ImageDataListToGroup(images?.Posters),
            Logos = ImageDataListToGroup(images?.Logos),
            Overview = tv.Overview,
            AirDate = tv.FirstAirDate,
            ExternalIds = new Dictionary<string, string>
            {
                [Info.ProviderId] = tv.Id.ToString()
            }
        };
    }
}