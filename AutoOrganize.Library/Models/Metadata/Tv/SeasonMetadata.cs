using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models.Metadata.Images;
using AutoOrganize.Library.Models.Metadata.Interfaces;

namespace AutoOrganize.Library.Models.Metadata.Tv;

public sealed class SeasonMetadata : TvMetadata, IPosters
{
    public SeriesMetadata? Series { get; set; }

    public int? SeasonNumber { get; set; }

    public ImageGroup? Posters { get; set; }

    private readonly List<EpisodeMetadata> _children = [];

    public IReadOnlyList<EpisodeMetadata> Children => _children;

    public void AddChild(EpisodeMetadata child)
    {
        child.Season = this;
        _children.Add(child);
    }

    public override void Complement(MetadataBase other)
    {
        base.Complement(other);
        if (other is not SeasonMetadata otherSeason)
            return;

        if (otherSeason.SeasonNumber is not null)
            SeasonNumber ??= otherSeason.SeasonNumber;

        if (otherSeason.Posters is not null)
        {
            Posters ??= [];
            Posters.AddRange(otherSeason.Posters);
        }
    }

    [MemberNotNullWhen(true, nameof(Series))]
    public override bool IsComplete()
    {
        return base.IsComplete() && SeasonNumber >= 0;
    }
}