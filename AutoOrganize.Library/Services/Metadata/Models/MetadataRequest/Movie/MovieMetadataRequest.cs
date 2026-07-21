using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Movie;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Movie;

public sealed class MovieMetadataRequest : IMetadataRequest<MovieMetadataRequest, MovieMetadata>, IHasCache
{
    public string? Title { get; init; }

    public int? Year { get; init; }

    public string? Country { get; init; }

    public string? Language { get; init; }

    public string? ImageLanguages { get; init; }

    public ProviderIds? ProviderIds { get; init; }

    public IEnumerable<string> GetCacheNames()
    {
        if (!string.IsNullOrEmpty(Title))
            yield return $"movie_{Title}_{Year}_{Country}_{Language}_{ImageLanguages}";

        if (ProviderIds is null)
            yield break;

        foreach ((string providerId,string id) in ProviderIds)
            yield return $"movie_{providerId}_{id}_{Year}_{Country}_{Language}_{ImageLanguages}";
    }
}