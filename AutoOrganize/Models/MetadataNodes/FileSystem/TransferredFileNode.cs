using System.IO;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Models.MetadataNodes.FileSystem;

public sealed class TransferredFileNode : MetadataTreeNodeBase, IFullPath, ISubheading
{
    public override string? Title { get; }

    public override MetadataNodeType NodeType => MetadataNodeType.File;

    public string Subheading { get; }

    public string FullPath { get; }

    public string OutputPath { get; }

    public TransferredFileNode(string filePath, string outputPath)
    {
        FullPath = filePath;
        Subheading = Path.GetExtension(filePath);
        Title = Path.GetFileNameWithoutExtension(filePath);
        OutputPath = outputPath;
    }
}