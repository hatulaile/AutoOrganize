using System.IO;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Models.MetadataNodes.FileSystem;

public sealed class FailedDirectoryNode : MetadataTreeNodeBase, IFullPath
{
    public override string? Title { get; }

    public override MetadataNodeType NodeType => MetadataNodeType.Directory;

    public string FullPath { get; }

    public override bool HasChildren => true;

    public FailedDirectoryNode(string directoryPath)
    {
        FullPath = directoryPath;
        Title = Path.GetFileName(directoryPath);
    }

}