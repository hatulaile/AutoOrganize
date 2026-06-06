using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Models.MetadataNodes.Metadata;

public sealed class SeasonMetadataTreeNode : MetadataTreeNodeBase, ISubheading, IFileMetadata<SeasonMetadata>
{
    public override string Title => Metadata.Name ?? string.Empty;

    public override MetadataNodeType NodeType => MetadataNodeType.TvSeason;

    public SeasonMetadata Metadata { get; }

    public string Subheading => $"Season {Metadata.SeasonNumber}";

    public override bool HasChildren => true;

    public SeasonMetadataTreeNode(SeasonMetadata seasonMetadata)
    {
        Metadata = seasonMetadata;
    }
}