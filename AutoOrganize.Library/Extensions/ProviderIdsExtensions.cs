using AutoOrganize.Library.Services.Metadata.Models.Metadata;

namespace AutoOrganize.Library.Extensions;

public static class ProviderIdsExtensions
{
    extension(IProviderIds ids)
    {
        public string GetAllProviderCache() =>
            string.Join(',', ids.Select(x => $"{x.Key}:{x.Value}"));
    }
}