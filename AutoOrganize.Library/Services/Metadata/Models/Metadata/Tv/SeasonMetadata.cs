using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;
using AutoOrganize.Library.Utils;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;

public sealed class SeasonMetadata :
    TvMetadata<SeasonMetadata>,
    IPosters,
    IParentOf<SeasonMetadata, EpisodeMetadata>,
    IChildOf<SeasonMetadata, SeriesMetadata>
{
    public override MetadataType Type => MetadataType.TvSeason;

    public SeriesMetadata? Series { get; private set; }

    public int? SeasonNumber { get; set; }

    public ImageGroup? Posters { get; set; }

    private readonly List<EpisodeMetadata> _children = [];

    public IReadOnlyList<EpisodeMetadata> Children => _children;

    public void AddChild(EpisodeMetadata child) =>
        _children.Add(child);

    public void RemoveChild(EpisodeMetadata child) =>
        _children.Remove(child);

    SeriesMetadata? IChildOf<SeasonMetadata, SeriesMetadata>.Parent
    {
        get => Series;
        set => Series = value;
    }

    public override SeasonMetadata Merge(SeasonMetadata other)
    {
        base.Merge(other);

        SeasonNumber ??= other.SeasonNumber;
        Posters = ImageGroupUtils.Coalesce(Posters, other.Posters);

        return this;
    }
}