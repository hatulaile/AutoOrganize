using System.Globalization;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;
using AutoOrganize.Library.Utils;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;

public sealed class SeriesMetadata : TvMetadata<SeriesMetadata>,
    IParentOf<SeriesMetadata, SeasonMetadata>,
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

    public void AddChild(SeasonMetadata child) =>
        _children.Add(child);

    public void RemoveChild(SeasonMetadata child) =>
        _children.Remove(child);

    public override SeriesMetadata Merge(SeriesMetadata other)
    {
        base.Merge(other);

        OriginalName ??= other.OriginalName;
        InProduction ??= other.InProduction;

        Posters = ImageGroupUtils.Coalesce(Posters, other.Posters);
        Backdrops = ImageGroupUtils.Coalesce(Backdrops, other.Backdrops);
        Logos = ImageGroupUtils.Coalesce(Logos, other.Logos);

        Languages ??= other.Languages;
        Countries ??= other.Countries;

        return this;
    }
}