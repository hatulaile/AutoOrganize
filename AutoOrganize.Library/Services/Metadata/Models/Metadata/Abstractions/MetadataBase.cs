using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

public abstract class MetadataBase
{
    public abstract MetadataType Type { get; }

    public string? Name { get; set; }

    public string? Overview { get; set; }

    public DateTime? AirDate { get; set; }

    public IProviderIds ProviderIds { get; init; } = new ProviderIds();
}

public abstract class MetadataBase<TSelf> : MetadataBase, IMetadataResult<TSelf>
    where TSelf : MetadataBase<TSelf>
{
    public virtual TSelf Merge(TSelf other)
    {
        Name ??= other.Name;
        Overview ??= other.Overview;
        AirDate ??= other.AirDate;
        foreach (var (key, value) in other.ProviderIds)
            ProviderIds.TryAdd(key, value);
        return (TSelf)this;
    }

    IEnumerable<string> IMetadataResult<TSelf>.GetIdentityKeys() => GetIdentityKeys();

    protected virtual IEnumerable<string> GetIdentityKeys()
    {
        foreach ((string providerId, string id) in ProviderIds)
            yield return $"{Type}_{Name}_{providerId}_{id}";
    }
}