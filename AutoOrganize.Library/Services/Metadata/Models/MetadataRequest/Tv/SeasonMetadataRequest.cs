using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;

public sealed class SeasonMetadataRequest :
    IMetadataRequest<SeasonMetadataRequest, SeasonMetadata>,
    IHasParentRequest<SeriesMetadataRequest>,
    IHasCache
{
    public string? Name { get; init; }

    public required int SeasonNumber { get; init; }

    public string? Language { get; init; }

    public string? ImageLanguages { get; init; }

    public required ProviderIds? ProviderIds { get; init; }

    public ITypedRequest GetParentRequest()
        => new CacheTypedRequest<SeriesMetadataRequest, SeriesMetadata>(new SeriesMetadataRequest
        {
            Name = Name,
            Language = Language,
            ImageLanguages = ImageLanguages,
            ProviderIds = ProviderIds,
        });

    public IEnumerable<string> GetCacheNames()
    {
        if(!string.IsNullOrEmpty(Name))
            yield return $"tv_season_{Name}_{SeasonNumber}_{Language}_{ImageLanguages}";

        if (ProviderIds is null)
            yield break;

        foreach ((string providerId, string id) in ProviderIds)
            yield return $"tv_season_{providerId}_{id}_{SeasonNumber}_{Language}_{ImageLanguages}";
    }
}