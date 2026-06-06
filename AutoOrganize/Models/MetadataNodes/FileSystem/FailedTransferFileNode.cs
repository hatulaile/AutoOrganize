using System;
using System.IO;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Models.MetadataNodes.FileSystem;

public sealed class FailedTransferFileNode : MetadataTreeNodeBase, IFullPath, ISubheading, IFailedFile
{
    public string FullPath { get; }

    public override string? Title { get; }

    public override MetadataNodeType NodeType => MetadataNodeType.File;

    public string Subheading { get; }

    public string? OutputPath { get; }

    public Exception Exception { get; }

    public FailedTransferFileNode(string filePath, string? outputPath, Exception exception)
    {
        FullPath = filePath;
        Subheading = Path.GetExtension(filePath);
        Title = Path.GetFileNameWithoutExtension(filePath);
        OutputPath = outputPath;
        Exception = exception;
    }
}