using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;

namespace AutoOrganize.ViewModels.Abstractions;

public abstract class ProviderIdsViewModelBase : ViewModelBase
{
    public ObservableProviderIds? ProviderIds { get; set; }

    public string? TmdbId
    {
        get => GetProviderId(nameof(ProviderType.ThemovieDB));
        set => SetProviderId(nameof(ProviderType.ThemovieDB), value);
    }

    protected abstract bool CanSetProviderId { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string? GetProviderId(string providerId) =>
        ProviderIds?.TryGetValue(providerId, out string? value) is true ? value : null;

    private void SetProviderId(string providerId, string? id)
    {
        if (!CanSetProviderId)
            return;

        if (string.IsNullOrWhiteSpace(id))
        {
            ProviderIds?.Remove(providerId);
            return;
        }

        ProviderIds ??= InitProviderIds();
        ProviderIds[providerId] = id;
    }

    protected ObservableProviderIds InitProviderIds()
    {
        var providerIds = new ObservableProviderIds();
        providerIds.CollectionChanged += ProviderIdsOnCollectionChanged;
        return providerIds;
    }

    protected ObservableProviderIds InitProviderIds(IProviderIds? source)
    {
        var providerIds = new ObservableProviderIds(source?.Count ?? 0);
        if (source is not null)
        {
            foreach ((string providerId, string id) in source)
            {
                providerIds.Add(providerId, id);
            }
        }

        providerIds.CollectionChanged += ProviderIdsOnCollectionChanged;
        return providerIds;
    }

    private void ProviderIdsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var oldItems = e.OldItems?
            .Cast<KeyValuePair<string, string>>().Select(x => x.Key) ?? [];
        var newItems = e.NewItems?
            .Cast<KeyValuePair<string, string>>().Select(x => x.Key) ?? [];

        foreach (var providerId in oldItems.Union(newItems))
            UpdatePropertyFromProviderId(providerId);
    }

    private void UpdatePropertyFromProviderId(string providerId)
    {
        switch (providerId)
        {
            case nameof(ProviderType.ThemovieDB):
                OnPropertyChanged(nameof(TmdbId));
                return;
        }
    }
}