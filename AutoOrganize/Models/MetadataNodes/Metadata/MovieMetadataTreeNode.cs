using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Models.MetadataNodes.Metadata;

public sealed class MovieMetadataTreeNode : MetadataTreeNodeBase, ISubheading, IFileMetadata<MovieMetadata>
{
    public override string Title => Metadata.Name ?? string.Empty;

    public override MetadataNodeType NodeType => MetadataNodeType.Movie;

    public MovieMetadata Metadata { get; }

    public override bool HasChildren => true;

    public string Subheading => Metadata.OriginalName ?? string.Empty;

    public MovieMetadataTreeNode(MovieMetadata metadata)
    {
        Metadata = metadata;
    }
}