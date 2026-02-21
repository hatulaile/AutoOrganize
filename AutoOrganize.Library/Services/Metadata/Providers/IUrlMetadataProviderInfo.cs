using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Library.Services.Metadata.Providers;

public interface IUrlMetadataProviderInfo : IMetadataProviderInfo
{
    string HomeUrl { get; }

    bool TryGetUrl(string id, MetadataType metadataType, [NotNullWhen(true)] out string? uri);
}