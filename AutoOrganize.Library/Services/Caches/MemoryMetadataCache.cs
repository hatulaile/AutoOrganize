using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models.Metadata;
using Microsoft.Extensions.Caching.Memory;

namespace AutoOrganize.Library.Services.Caches;

public sealed class MemoryMetadataCache : IMetadataCache, IDisposable
{
    private readonly MemoryCache _memoryCache;

    public void Set(string key, MetadataBase value)
    {
        _memoryCache.Set(key, value);
    }

    public bool TryGet<TMetadata>(string key, [NotNullWhen(true)] out TMetadata? metadata)
        where TMetadata : MetadataBase
    {
        return _memoryCache.TryGetValue(key, out metadata);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public void Clear()
    {
        _memoryCache.Clear();
    }

    public MemoryMetadataCache(MemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (disposing)
        {
            _memoryCache.Dispose();
        }
    }

    ~MemoryMetadataCache()
    {
        Dispose(false);
    }
}