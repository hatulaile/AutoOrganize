using AutoOrganize.Library.Models.Metadata.Tv;

namespace AutoOrganize.Models.MetadataViewModels.Metadata;

public sealed class FileEpisodeMetadata : FileMetadataBase, ISubheading, IFileMetadata<EpisodeMetadata>
{
    public override string Title => Metadata.Name ?? string.Empty;

    public override FileMetadataType Type => FileMetadataType.TvEpisode;

    public override bool HasChildren => true;

    public string Subheading => $"Episode {Metadata.EpisodeNumber}";

    public EpisodeMetadata Metadata { get; }

    public FileEpisodeMetadata(EpisodeMetadata episodeMetadata)
    {
        Metadata = episodeMetadata;
    }
}