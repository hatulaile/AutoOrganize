using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;

public sealed class SeriesMetadataRequest : IMetadataRequest<SeriesMetadataRequest, SeriesMetadata>, IHasCache
{
    public string? Name { get; init; }

    public string? Language { get; init; }

    public string? ImageLanguages { get; init; }

    public ProviderIds? ProviderIds { get; init; }


    public IEnumerable<string> GetCacheNames()
    {
        if (string.IsNullOrEmpty(Name))
            yield return $"tv_series_{Name}_{Language}_{ImageLanguages}";

        if (ProviderIds is null)
            yield break;

        foreach ((string providerId, string id) in ProviderIds)
            yield return $"tv_series_{providerId}_{id}_{Language}";
    }
}