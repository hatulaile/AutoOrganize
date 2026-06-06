using System.IO;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Models.MetadataNodes.FileSystem;

public class SourceFileNode : MetadataTreeNodeBase, IFullPath, ISubheading
{
    public override string? Title { get; }

    public override MetadataNodeType NodeType => MetadataNodeType.File;

    public string FullPath { get; }

    public string Subheading { get; }

    public SourceFileNode(string fullPath)
    {
        FullPath = fullPath;
        Subheading = Path.GetExtension(fullPath);
        Title = Path.GetFileNameWithoutExtension(fullPath);
    }
}