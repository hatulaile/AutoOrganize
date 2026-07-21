using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface IUriProviderInfo : IProviderInfo
{
    public Uri? HomeUri { get; }

    bool TryGetUri(string id, MetadataType type,[NotNullWhen(true)] out Uri? uri);
}