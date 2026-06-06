using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Models.MetadataNodes.Metadata;

public sealed class EpisodeMetadataTreeNode : MetadataTreeNodeBase, ISubheading, IFileMetadata<EpisodeMetadata>
{
    public override string Title => Metadata.Name ?? string.Empty;

    public override MetadataNodeType NodeType => MetadataNodeType.TvEpisode;

    public override bool HasChildren => true;

    public string Subheading => $"Episode {Metadata.EpisodeNumber}";

    public EpisodeMetadata Metadata { get; }

    public EpisodeMetadataTreeNode(EpisodeMetadata episodeMetadata)
    {
        Metadata = episodeMetadata;
    }
}