using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AutoOrganize.Library.Models.Metadata.Images;
using AutoOrganize.Library.Models.Metadata.Interfaces;

namespace AutoOrganize.Library.Models.Metadata.Movie;

public sealed class MovieMetadata : MetadataBase,
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

    public override void Complement(MetadataBase other)
    {
        base.Complement(other);
        if (other is not MovieMetadata movie) return;

        if (movie.OriginalName is not null)
            OriginalName ??= movie.OriginalName;

        if (movie.Runtime is not null)
            Runtime = movie.Runtime;

        if (movie.Revenue is not null)
            Revenue = movie.Revenue;

        if (movie.Backdrops is not null)
        {
            Backdrops ??= [];
            Backdrops.AddRange(movie.Backdrops);
        }

        if (movie.Posters is not null)
        {
            Posters ??= [];
            Posters.AddRange(movie.Posters);
        }

        if (movie.Logos is not null)
        {
            Logos ??= [];
            Logos.AddRange(movie.Logos);
        }

        if (movie.Languages is not null)
        {
            Languages ??= [];
            Languages.AddRange(movie.Languages.Except(Languages));
        }

        if (movie.Countries is not null)
        {
            Countries ??= [];
            Countries.AddRange(movie.Countries.Except(Countries));
        }
    }

    [MemberNotNullWhen(true, nameof(OriginalName))]
    public override bool IsComplete()
    {
        return base.IsComplete() && OriginalName is not null;
    }
}