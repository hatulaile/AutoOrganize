using System.Collections.Generic;
using System.Linq;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata.Images;
using AutoOrganize.Library.Services.Metadata.Providers;
using Avalonia.Data.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize.Converters;

public static class MetadataConverters
{
    public static FuncValueConverter<string, string> MetadataProvidersIConConverter { get; } = new(providerName =>
    {
        if (providerName is null)
            return string.Empty;

        return providerName switch
        {
            nameof(MetadataProviderType.ThemovieDB) => "/Assets/Images/TheMovieDB/PrimaryFull.svg",
            _ => string.Empty
        };
    });

    public static FuncValueConverter<ImageGroup, string?> ImageGroupUrlConverter { get; } = new(GetImageGroupUrl);

    public static string? GetImageGroupUrl(ImageGroup? groups)
    {
        if (groups is null) return null;
        string? highestPriorityId = GetHighestPriorityId(App.Current.ServiceProvider.GetServices<IMetadataProvider>(),
            groups.ImageDataListForId
                .Where(x => x.Value.ImageData.Count > 0).Select(x => x.Key));
        if (highestPriorityId is null)
            return null;

        ImageDataListBase list = groups.ImageDataListForId[highestPriorityId];
        return list.ImageData.MaxBy(x => x.Priority)?.ImageUrl;
    }

    private static string? GetHighestPriorityId(IEnumerable<IMetadataProvider> providers,
        IEnumerable<string> providerIds)
    {
        (int order, string? providerId) highestPriority = (int.MinValue, null);

        var allProvider = providers.ToArray();

        foreach (string providerId in providerIds)
        {
            var config = allProvider.FirstOrDefault(x => x.Info.ProviderId.Equals(providerId))?.Config;
            if (config is null)
            {
                if (highestPriority.providerId is null)
                    highestPriority = (int.MinValue, providerId);
                continue;
            }

            if (highestPriority.order >= config.Priority) continue;
            highestPriority = (config.Priority, providerId);
        }

        return highestPriority.providerId;
    }
}