using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models.Metadata.Images;
using AutoOrganize.Library.Models.Metadata.Interfaces;

namespace AutoOrganize.Library.Models.Metadata.Tv;

public sealed class EpisodeMetadata : TvMetadata, IBackdrops
{
    public SeriesMetadata? Series => Season?.Series;

    public SeasonMetadata? Season { get; set; }

    public ImageGroup? Backdrops { get; set; }

    public int? SeasonNumber => Season?.SeasonNumber;

    public long? EpisodeNumber { get; set; }

    public override void Complement(MetadataBase other)
    {
        base.Complement(other);
        if (other is not EpisodeMetadata episode) return;

        if (episode.EpisodeNumber is not null)
            EpisodeNumber = episode.EpisodeNumber;

        if (episode.Backdrops is not null)
        {
            Backdrops ??= [];
            Backdrops.AddRange(episode.Backdrops);
        }
    }

    [MemberNotNullWhen(true, nameof(Series), nameof(Season))]
    public override bool IsComplete()
    {
        return base.IsComplete() && EpisodeNumber >= 0L;
    }
}