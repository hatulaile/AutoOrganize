using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Models.MetadataNodes.Metadata;

public sealed class SeriesMetadataTreeNode : MetadataTreeNodeBase, ISubheading, IFileMetadata<SeriesMetadata>
{
    public override string Title => Metadata.Name ?? string.Empty;

    public override MetadataNodeType NodeType => MetadataNodeType.TvSeries;

    public SeriesMetadata Metadata { get; }

    public string Subheading => Metadata.OriginalName ?? string.Empty;

    public override bool HasChildren => true;

    public SeriesMetadataTreeNode(SeriesMetadata seasonMetadata)
    {
        Metadata = seasonMetadata;
    }
}