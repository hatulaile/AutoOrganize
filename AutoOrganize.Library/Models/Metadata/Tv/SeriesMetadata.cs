using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AutoOrganize.Library.Models.Metadata.Images;
using AutoOrganize.Library.Models.Metadata.Interfaces;

namespace AutoOrganize.Library.Models.Metadata.Tv;

public sealed class SeriesMetadata : TvMetadata,
    IOriginalName, IBackdrops, IPosters, ILogos, ILanguages, ICountries
{
    public string? OriginalName { get; set; }

    public bool? InProduction { get; set; }

    public List<CultureInfo>? Languages { get; set; }

    public List<RegionInfo>? Countries { get; set; }

    public ImageGroup? Backdrops { get; set; }

    public ImageGroup? Posters { get; set; }

    public ImageGroup? Logos { get; set; }

    private readonly List<SeasonMetadata> _children = [];

    public IReadOnlyList<SeasonMetadata> Children => _children;

    public void AddChild(SeasonMetadata child)
    {
        child.Series = this;
        _children.Add(child);
    }

    public override void Complement(MetadataBase other)
    {
        base.Complement(other);
        if (other is not SeriesMetadata series)
            return;

        if (series.InProduction is not null)
            InProduction ??= series.InProduction;

        if (series.Backdrops is not null)
        {
            Backdrops ??= [];
            Backdrops.AddRange(series.Backdrops);
        }

        if (series.Posters is not null)
        {
            Posters ??= [];
            Posters.AddRange(series.Posters);
        }

        if (series.Logos is not null)
        {
            Logos ??= [];
            Logos.AddRange(series.Logos);
        }

        if (series.Languages is not null)
        {
            Languages ??= [];
            Languages.AddRange(series.Languages.Except(Languages));
        }

        if (series.Countries is not null)
        {
            Countries ??= [];
            Countries.AddRange(series.Countries.Except(Countries));
        }
    }

    [MemberNotNullWhen(true, nameof(OriginalName))]
    public override bool IsComplete()
    {
        return base.IsComplete() && OriginalName is not null;
    }
}