using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Models.Metadata;

public abstract class MetadataBase
{
    public abstract MetadataType Type { get; }

    public string? Name { get; set; }

    public string? Overview { get; set; }

    public DateTime? AirDate { get; set; }

    public Dictionary<string, string>? ExternalIds { get; set; }

    public virtual void Complement(MetadataBase other)
    {
        if (other.Name is not null)
            Name ??= other.Name;
        if (other.Overview is not null)
            Overview ??= other.Overview;
        if (other.AirDate is not null)
            AirDate ??= other.AirDate;

        if (other.ExternalIds is not null)
        {
            ExternalIds ??= [];
            foreach (var id in other.ExternalIds)
                ExternalIds.Add(id.Key, id.Value);
        }
    }

    [MemberNotNullWhen(true, nameof(Name), nameof(Overview), nameof(AirDate))]
    public virtual bool IsComplete()
    {
        return Name is not null && Overview is not null && AirDate is not null;
    }
}