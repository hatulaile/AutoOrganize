using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Library.Services.Caches;

public interface IMetadataCache
{
    void Set(string key, MetadataBase value);

    public bool TryGet<TMetadata>(string key, [NotNullWhen(true)] out TMetadata? metadata) where TMetadata : MetadataBase;

    void Remove(string key);

    void Clear();
}