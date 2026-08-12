using System.Globalization;
using AutoOrganize.Library.Extensions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;
using AutoOrganize.Library.Utils;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;

public sealed class SeriesMetadata : TvMetadata<SeriesMetadata>,
    IParentOf<SeriesMetadata, SeasonMetadata>,
    IOriginalName, IBackdrops, IPosters, ILogos, ILanguages, ICountries
{
    public override MetadataType Type => MetadataType.TvSeries;

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

        if (other.Languages is not null)
        {
            if (Languages is null) Languages = other.Languages;
            else Languages.AddRangeIfNotExists(other.Languages);
        }

        if (other.Countries is not null)
        {
            if (Countries is null) Countries = other.Countries;
            else Countries.AddRangeIfNotExists(other.Countries);
        }

        return this;
    }
}