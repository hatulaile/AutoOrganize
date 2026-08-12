using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface IUriProviderInfo : IProviderInfo
{
    public Uri? HomeUri { get; }

    bool TryGetUri(MetadataBase metadataBase, [NotNullWhen(true)] out Uri? uri);
}