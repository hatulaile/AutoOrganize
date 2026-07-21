using System.Globalization;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;
using AutoOrganize.Library.Utils;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Movie;

public sealed class MovieMetadata : MetadataBase<MovieMetadata>,
    IRuntime, IRevenue, IOriginalName, IBackdrops, IPosters, ILogos, ILanguages, ICountries
{
    public override MetadataType Type => MetadataType.Movie;
    public string? OriginalName { get; set; }

    public int? Runtime { get; set; }

    public long? Revenue { get; set; }

    public ImageGroup? Backdrops { get; set; }

    public ImageGroup? Posters { get; set; }

    public ImageGroup? Logos { get; set; }

    public List<CultureInfo>? Languages { get; set; }

    public List<RegionInfo>? Countries { get; set; }

    public override MovieMetadata Merge(MovieMetadata other)
    {
        base.Merge(other);

        OriginalName ??= other.OriginalName;
        Runtime ??= other.Runtime;
        Revenue ??= other.Revenue;

        Posters = ImageGroupUtils.Coalesce(Posters, other.Posters);
        Backdrops = ImageGroupUtils.Coalesce(Backdrops, other.Backdrops);
        Logos = ImageGroupUtils.Coalesce(Logos, other.Logos);

        Languages ??= other.Languages;
        Countries ??= other.Countries;

        return this;
    }
}