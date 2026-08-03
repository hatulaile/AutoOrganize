using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;
using AutoOrganize.Library.Services.Metadata.Providers.Abstractions;
using AutoOrganize.Library.Services.RequestCoalescers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AutoOrganize.Library.Services.Metadata;

public sealed class MetadataService : IMetadataService, IMetadataFetchService
{
    private readonly IProviderService _providerService;
    private readonly IMemoryCache _metadataCache;
    private readonly IFlightCoordinator _flightCoordinator;
    private readonly ILogger<MetadataService> _logger;

    public MetadataService(IProviderService providerService, IFlightCoordinator flightCoordinator,
        ILogger<MetadataService> logger)
    {
        _providerService = providerService;
        _flightCoordinator = flightCoordinator;
        _logger = logger;
        _metadataCache = new MemoryCache(new MemoryCacheOptions());
    }

    public async Task<TResult[]?> SearchResultsAsync<TRequest, TResult>(TRequest request,
        CancellationToken token = default)
        where TResult : ISearchResult<TResult>
        where TRequest : ISearchRequest<TRequest, TResult>
    {
        return await FetchAndMergeSearchResults<TRequest, TResult>(request, false, token).ConfigureAwait(false);
    }

    public async Task<TResult[]?> SearchResultsAsync<TRequest, TResult>(TRequest info, bool ignoreCache,
        CancellationToken token = default)
        where TResult : ISearchResult<TResult>
        where TRequest : ISearchRequest<TRequest, TResult>, IHasCache
    {
        ILease? lease;
        string[] cacheNames = [.. info.GetCacheNames()];
        do
        {
            if (!ignoreCache && GetCachedSearchResults<TResult>(cacheNames) is { } results)
                return results;

            (bool acquired, lease) =
                await _flightCoordinator.AcquireAsync(cacheNames, token).ConfigureAwait(false);

            if (acquired)
                break;
        } while (true);

        try
        {
            TResult[]? results = await FetchAndMergeSearchResults<TRequest, TResult>(info, ignoreCache, token)
                .ConfigureAwait(false);
            if (results is null)
                return null;
            TryCacheSearchResults(cacheNames, results);
            return results;
        }
        finally
        {
            lease?.Dispose();
        }
    }

    public async Task<TResult?> GetMetadataAsync<TRequest, TResult>(TRequest request, CancellationToken token = default)
        where TResult : IMetadataResult<TResult>
        where TRequest : IMetadataRequest<TRequest, TResult>
    {
        return await FetchAndMergeMetadataAsync<TRequest, TResult>(request, false, token).ConfigureAwait(false);
    }

    public async Task<TResult?> GetMetadataAsync<TRequest, TResult>(TRequest request, bool ignoreCache,
        CancellationToken token = default)
        where TResult : IMetadataResult<TResult>
        where TRequest : IMetadataRequest<TRequest, TResult>, IHasCache
    {
        return await GetMetadataWithCacheAsync<TRequest, TResult>(request, ignoreCache, token).ConfigureAwait(false);
    }

    public async Task<TResult?> GetMetadataAsync<TRequest, TResult, TParent>(
        TRequest request, CancellationToken token = default)
        where TResult : IChildOf<TResult, TParent>, IMetadataResult<TResult>
        where TParent : IParentOf<TParent, TResult>, IMetadataResult<TParent>
        where TRequest : IMetadataRequest<TRequest, TResult>, IHasParentRequest
    {
        return await FetchAndMergeWithParentAsync<TRequest, TResult, TParent>(request, false, token)
            .ConfigureAwait(false);
    }

    public async Task<TResult?> GetMetadataAsync<TRequest, TResult, TParent>(
        TRequest request, bool ignoreCache, CancellationToken token = default)
        where TResult : IChildOf<TResult, TParent>, IMetadataResult<TResult>
        where TParent : IParentOf<TParent, TResult>, IMetadataResult<TParent>
        where TRequest : IMetadataRequest<TRequest, TResult>, IHasParentRequest, IHasCache
    {
        return await GetMetadataWithCacheAndParentAsync<TRequest, TResult, TParent>(
                request, ignoreCache, token)
            .ConfigureAwait(false);
    }

    private async Task<TResult[]?> FetchAndMergeSearchResults<TRequest, TResult>(TRequest request,
        bool ignoreCache = false,
        CancellationToken token = default)
        where TResult : ISearchResult<TResult>
        where TRequest : ISearchRequest<TRequest, TResult>
    {
        List<TResult> results = [];
        foreach (var provider in _providerService.GetSearchProviders<TRequest, TResult>())
        {
            IEnumerable<TResult>? providerResult;
            if (provider is IHasCacheSearchProvider<TRequest, TResult> hasCacheProvider)
                providerResult = await hasCacheProvider.SearchAsync(request, ignoreCache, token).ConfigureAwait(false);
            else providerResult = await provider.SearchAsync(request, token).ConfigureAwait(false);

            if (providerResult is null)
                continue;
            foreach (TResult searchResult in providerResult)
            {
                TResult? sourceResult = results.FirstOrDefault(x =>
                {
                    foreach (var providerId in searchResult.ProviderIds)
                    {
                        if (x.ProviderIds.TryGetValue(providerId.Key, out var p) && p.Equals(providerId.Value))
                            return true;
                    }

                    return false;
                });

                if (sourceResult is null)
                {
                    results.Add(searchResult);
                    continue;
                }

                foreach ((string providerId, string id) in searchResult.ProviderIds)
                    searchResult.ProviderIds.TryAdd(providerId, id);
            }
        }

        return results.Count > 0 ? [.. results] : null;
    }

    private TResult[]? GetCachedSearchResults<TResult>(IEnumerable<string> cacheNames)
        where TResult : ISearchResult<TResult>
    {
        foreach (string cacheName in cacheNames)
        {
            if (_metadataCache.TryGetValue(cacheName, out TResult[]? cached))
                return cached;
        }

        return null;
    }

    private void TryCacheSearchResults<TResult>(IEnumerable<string> cacheNames, TResult[] results)
        where TResult : ISearchResult<TResult>
    {
        foreach (string name in cacheNames)
        {
            if (_metadataCache.TryGetValue(name, out _))
                continue;
            _metadataCache.Set(name, results, TimeSpan.FromMinutes(5L));
        }
    }

    private async Task<TResult?> FetchAndMergeMetadataAsync<TRequest, TResult>(TRequest request,
        bool ignoreCache = false,
        CancellationToken token = default)
        where TResult : IMetadataResult<TResult>
        where TRequest : IMetadataRequest<TRequest, TResult>
    {
        TResult? result = default;
        foreach (var provider in _providerService.GetMetadataProviders<TRequest, TResult>())
        {
            TResult? providerResult;
            if (provider is IHasCacheMetadataProvider<TRequest, TResult> hasCacheProvider)
                providerResult = await hasCacheProvider.GetMetadataAsync(request, ignoreCache, token)
                    .ConfigureAwait(false);
            else providerResult = await provider.GetMetadataAsync(request, token).ConfigureAwait(false);
            if (providerResult is null) continue;
            result = result is null ? providerResult : result.Merge(providerResult);
        }

        return result;
    }

    private async Task<TResult?> GetMetadataWithCacheAsync<TRequest, TResult>(
        TRequest request, bool ignoreCache, CancellationToken token)
        where TResult : IMetadataResult<TResult>
        where TRequest : IMetadataRequest<TRequest, TResult>, IHasCache
    {
        string[] cacheNames = [.. request.GetCacheNames()];

        ILease? lease;
        do
        {
            if (!ignoreCache && GetCachedMetadata<TResult>(cacheNames) is { } result)
                return result;

            (bool acquired, lease) =
                await _flightCoordinator.AcquireAsync(request.GetCacheNames(), token).ConfigureAwait(false);

            if (acquired)
                break;
        } while (true);

        try
        {
            TResult? result = await FetchAndMergeMetadataAsync<TRequest, TResult>(request, ignoreCache, token)
                .ConfigureAwait(false);
            if (result is not null)
            {
                result = await DeduplicateResultAsync(cacheNames, result, token).ConfigureAwait(false);
            }

            return result;
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private async Task<TResult?> FetchAndMergeWithParentAsync<TRequest, TResult, TParent>(
        TRequest request, bool ignoreCache, CancellationToken token)
        where TResult : IChildOf<TResult, TParent>, IMetadataResult<TResult>
        where TParent : IParentOf<TParent, TResult>, IMetadataResult<TParent>
        where TRequest : IMetadataRequest<TRequest, TResult>, IHasParentRequest
    {
        TResult? metadata = await FetchAndMergeMetadataAsync<TRequest, TResult>(request, ignoreCache, token)
            .ConfigureAwait(false);
        if (metadata is null) return default;

        IChildOf? child = metadata;
        IHasParentRequest currentRequest = request;
        do
        {
            ITypedRequest parentRequest = currentRequest.GetParentRequest();
            IParentOf? parent;
            if (parentRequest is ICacheTypedRequest cacheRequest)
            {
                parent = (IParentOf?)await cacheRequest.FetchAsync(this, ignoreCache, token)
                    .ConfigureAwait(false);
            }
            else
            {
                parent = (IParentOf?)await parentRequest.FetchAsync(this, token).ConfigureAwait(false);
            }

            if (parent is null)
                return metadata;

            child.Parent = parent;
            parent.AddChild(child);

            if (parentRequest.Request is not IHasParentRequest hasParentRequest)
                return metadata;

            currentRequest = hasParentRequest;
            child = parent as IChildOf;
            if (child is null) return metadata;
        } while (true);
    }

    async Task<TResult?> IMetadataFetchService.FetchAsync<TRequest, TResult>(
        TRequest request, CancellationToken token)
        where TRequest : default
        where TResult : default
    {
        if (request is IHasCache)
            return await FetchWithCacheFallbackAsync<TRequest, TResult>(request, false, token)
                .ConfigureAwait(false);
        return await FetchAndMergeMetadataAsync<TRequest, TResult>(request, false, token).ConfigureAwait(false);
    }

    async Task<TResult?> IMetadataFetchService.FetchAsync<TRequest, TResult>(
        TRequest request, bool ignoreCache, CancellationToken token)
        where TRequest : default
        where TResult : default
    {
        return await FetchWithCacheFallbackAsync<TRequest, TResult>(request, ignoreCache, token)
            .ConfigureAwait(false);
    }

    private async Task<TResult?> FetchWithCacheFallbackAsync<TRequest, TResult>(
        TRequest request, bool ignoreCache, CancellationToken token)
        where TRequest : IMetadataRequest<TRequest, TResult>
        where TResult : IMetadataResult<TResult>
    {
        string[] cacheNames = [.. ((IHasCache)request).GetCacheNames()];

        ILease? lease;
        do
        {
            if (!ignoreCache && GetCachedMetadata<TResult>(cacheNames) is { } cached)
                return cached;

            (bool acquired, lease) =
                await _flightCoordinator.AcquireAsync(cacheNames, token).ConfigureAwait(false);

            if (acquired)
                break;
        } while (true);

        try
        {
            TResult? result = await FetchAndMergeMetadataAsync<TRequest, TResult>(request, ignoreCache, token)
                .ConfigureAwait(false);
            if (result is not null)
            {
                result = await DeduplicateResultAsync(cacheNames, result, token).ConfigureAwait(false);
            }

            return result;
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private async Task<TResult?> GetMetadataWithCacheAndParentAsync<TRequest, TResult, TParent>(
        TRequest request, bool ignoreCache, CancellationToken token)
        where TResult : IChildOf<TResult, TParent>, IMetadataResult<TResult>
        where TParent : IParentOf<TParent, TResult>, IMetadataResult<TParent>
        where TRequest : IMetadataRequest<TRequest, TResult>, IHasParentRequest, IHasCache
    {
        string[] cacheNames = [.. request.GetCacheNames()];

        ILease? lease;
        do
        {
            if (!ignoreCache && GetCachedMetadata<TResult>(cacheNames) is { } result)
                return result;

            (bool acquired, lease) =
                await _flightCoordinator.AcquireAsync(cacheNames, token).ConfigureAwait(false);

            if (acquired)
                break;
        } while (true);

        try
        {
            TResult? fetched =
                await FetchAndMergeWithParentAsync<TRequest, TResult, TParent>(request, ignoreCache, token)
                    .ConfigureAwait(false);
            if (fetched is null) return default;

            var deduped = await DeduplicateResultAsync(cacheNames, fetched, token).ConfigureAwait(false);
            if (!ReferenceEquals(fetched, deduped) && fetched is IChildOf { Parent: IParentOf oldParent } child)
            {
                oldParent.RemoveChild(child);
                child.Parent = null;
            }

            return deduped;
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private async Task<TResult> DeduplicateResultAsync<TResult>(
        IEnumerable<string> requestCacheNames, TResult result, CancellationToken token)
        where TResult : IMetadataResult<TResult>
    {
        string[] identityKeys = [.. result.GetIdentityKeys()];
        if (identityKeys.Length == 0)
        {
            TryCacheMetadata(requestCacheNames, result);
            return result;
        }

        ILease? identityLease;
        do
        {
            var existing = GetCachedMetadata<TResult>(identityKeys);
            if (existing is not null)
            {
                existing.Merge(result);
                TryCacheMetadata(requestCacheNames, existing);
                return existing;
            }

            (bool acquired, identityLease) = await _flightCoordinator
                .AcquireAsync(identityKeys, token).ConfigureAwait(false);

            if (acquired) break;
        } while (true);

        try
        {
            var existing = GetCachedMetadata<TResult>(identityKeys);
            if (existing is not null)
            {
                existing.Merge(result);
                TryCacheMetadata(requestCacheNames, existing);
                return existing;
            }

            TryCacheMetadata(identityKeys, result);
            TryCacheMetadata(requestCacheNames, result);
            return result;
        }
        finally
        {
            identityLease?.Dispose();
        }
    }

    private TResult? GetCachedMetadata<TResult>(IEnumerable<string> cacheNames)
        where TResult : IMetadataResult<TResult>
    {
        foreach (string cacheName in cacheNames)
        {
            if (_metadataCache.TryGetValue(cacheName, out TResult? cached))
                return cached;
        }

        return default;
    }

    private void TryCacheMetadata<TResult>(IEnumerable<string> cacheNames, TResult result)
        where TResult : IMetadataResult<TResult>
    {
        foreach (string name in cacheNames)
        {
            if (_metadataCache.TryGetValue(name, out _))
                continue;
            _metadataCache.Set(name, result, TimeSpan.FromMinutes(10L));
        }
    }
}