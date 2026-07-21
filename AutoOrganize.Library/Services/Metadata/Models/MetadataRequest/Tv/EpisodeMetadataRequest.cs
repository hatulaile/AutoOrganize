using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;

public sealed class EpisodeMetadataRequest :
    IMetadataRequest<EpisodeMetadataRequest, EpisodeMetadata>,
    IHasParentRequest<SeasonMetadataRequest>,
    IHasCache
{
    public string? Name { get; init; }

    public required int SeasonNumber { get; init; }

    public required long EpisodeNumber { get; init; }

    public string? Language { get; init; }

    public string? ImageLanguages { get; init; }

    public ProviderIds? ProviderIds { get; init; }

    public ITypedRequest GetParentRequest()
        => new CacheTypedRequest<SeasonMetadataRequest, SeasonMetadata>(new SeasonMetadataRequest
        {
            Name = Name,
            SeasonNumber = SeasonNumber,
            Language = Language,
            ImageLanguages = ImageLanguages,
            ProviderIds = ProviderIds
        });

    public IEnumerable<string> GetCacheNames()
    {
        if(!string.IsNullOrEmpty(Name))
            yield return $"tv_episode_{Name}_{SeasonNumber}_{EpisodeNumber}_{Language}_{ImageLanguages}";

        if (ProviderIds is null)
            yield break;

        foreach ((string providerId, string id) in ProviderIds)
            yield return $"tv_episode_{providerId}_{id}_{SeasonNumber}_{EpisodeNumber}_{Language}_{ImageLanguages}";
    }
}