using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;

public sealed class EpisodeMetadataRequest :
    IMetadataRequest<EpisodeMetadataRequest, EpisodeMetadata>,
    IHasParentRequest<SeasonMetadataRequest, SeasonMetadata>,
    IHasCache
{
    public string? SeriesName { get; set; }

    public int? Year { get; set; }

    public int SeasonNumber { get; set; }

    public long EpisodeNumber { get; set; }

    public string? Language { get; set; }

    public string? ImageLanguages { get; set; }

    public IProviderIds? SeriesProviderIds { get; set; }

    public IProviderIds? ProviderIds { get; set; }

    public ITypedRequest<SeasonMetadataRequest, SeasonMetadata> GetParentRequest()
        => new CacheTypedRequest<SeasonMetadataRequest, SeasonMetadata>(new SeasonMetadataRequest
        {
            SeriesName = SeriesName,
            SeasonNumber = SeasonNumber,
            Language = Language,
            ImageLanguages = ImageLanguages,
            SeriesProviderIds = SeriesProviderIds,
            ProviderIds = ProviderIds
        });

    public IEnumerable<string> GetCacheNames()
    {
        if (!string.IsNullOrEmpty(SeriesName))
            yield return $"tv_episode_{SeriesName}_{Year}_{SeasonNumber}_{EpisodeNumber}_{Language}_{ImageLanguages}";

        if (ProviderIds is not null && SeriesProviderIds is not null)
        {
            foreach ((string seriesProviderId, string seriesId) in SeriesProviderIds)
                foreach ((string providerId, string id) in ProviderIds)
                    yield return
                        $"tv_episode_{seriesProviderId}_{seriesId}_{providerId}_{id}_{Year}_{SeasonNumber}_{EpisodeNumber}_{Language}_{ImageLanguages}";
        }

        if (SeriesProviderIds is null && ProviderIds is not null)
        {
            foreach ((string providerId, string id) in ProviderIds)
                yield return
                    $"tv_episode_{providerId}_{id}_{Year}_{SeasonNumber}_{EpisodeNumber}_{Language}_{ImageLanguages}";
            yield break;
        }

        if (ProviderIds is null && SeriesProviderIds is not null)
        {
            foreach ((string providerId, string id) in SeriesProviderIds)
                yield return
                    $"tv_episode_{providerId}_{id}_{Year}_{SeasonNumber}_{EpisodeNumber}_{Language}_{ImageLanguages}";
            yield break;
        }
    }
}