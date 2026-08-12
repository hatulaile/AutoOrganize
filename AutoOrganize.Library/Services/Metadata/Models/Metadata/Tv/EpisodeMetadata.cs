using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;
using AutoOrganize.Library.Utils;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;

public sealed class EpisodeMetadata :
    TvMetadata<EpisodeMetadata>,
    IBackdrops,
    IChildOf<EpisodeMetadata, SeasonMetadata>
{
    public override MetadataType Type => MetadataType.TvEpisode;

    public SeriesMetadata? Series => Season?.Series;

    public SeasonMetadata? Season { get; private set; }

    public ImageGroup? Backdrops { get; set; }

    public int? SeasonNumber => Season?.SeasonNumber;

    public long? EpisodeNumber { get; set; }

    SeasonMetadata? IChildOf<EpisodeMetadata, SeasonMetadata>.Parent
    {
        get => Season;
        set => Season = value;
    }

    public override EpisodeMetadata Merge(EpisodeMetadata other)
    {
        base.Merge(other);

        EpisodeNumber ??= other.EpisodeNumber;
        Backdrops = ImageGroupUtils.Coalesce(Backdrops, other.Backdrops);

        return this;
    }
}