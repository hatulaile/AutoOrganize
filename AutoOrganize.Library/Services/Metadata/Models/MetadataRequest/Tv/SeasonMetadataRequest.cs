using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;

public sealed class SeasonMetadataRequest :
    IMetadataRequest<SeasonMetadataRequest, SeasonMetadata>,
    IHasParentRequest<SeriesMetadataRequest,SeriesMetadata>,
    IHasCache
{
    public string? SeriesName { get; set; }

    public int SeasonNumber { get; set; }

    public string? Language { get; set; }

    public string? ImageLanguages { get; set; }

    public required IProviderIds? SeriesProviderIds { get; set; }

    public required IProviderIds? ProviderIds { get; set; }

    public ITypedRequest<SeriesMetadataRequest,SeriesMetadata> GetParentRequest()
        => new CacheTypedRequest<SeriesMetadataRequest, SeriesMetadata>(new SeriesMetadataRequest
        {
            Name = SeriesName,
            Language = Language,
            ImageLanguages = ImageLanguages,
            ProviderIds = SeriesProviderIds,
        });

    public IEnumerable<string> GetCacheNames()
    {
        if (!string.IsNullOrEmpty(SeriesName))
            yield return $"tv_season_{SeriesName}_{SeasonNumber}_{Language}_{ImageLanguages}";

        if (ProviderIds is null)
            yield break;

        if (ProviderIds is not null && SeriesProviderIds is not null)
        {
            foreach ((string seriesProviderId, string seriesId) in SeriesProviderIds)
                foreach ((string providerId, string id) in ProviderIds)
                    yield return
                        $"tv_season_{seriesProviderId}_{seriesId}_{providerId}_{id}_{SeasonNumber}_{Language}_{ImageLanguages}";
        }

        if (SeriesProviderIds is null && ProviderIds is not null)
        {
            foreach ((string providerId, string id) in ProviderIds)
                yield return
                    $"tv_season_{providerId}_{id}_{SeasonNumber}_{Language}_{ImageLanguages}";
            yield break;
        }

        if (ProviderIds is null && SeriesProviderIds is not null)
        {
            foreach ((string providerId, string id) in SeriesProviderIds)
                yield return
                    $"tv_season_{providerId}_{id}_{SeasonNumber}_{Language}_{ImageLanguages}";
            yield break;
        }
    }
}