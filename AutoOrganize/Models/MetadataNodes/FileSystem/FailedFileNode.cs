using System;
using System.IO;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Models.MetadataNodes.FileSystem;

public sealed class FailedFileNode : MetadataTreeNodeBase, IFullPath, IFailedFile
{
    public string FullPath { get; }

    public override string? Title { get; }

    public override MetadataNodeType NodeType => MetadataNodeType.File;

    public Exception Exception { get; }

    public FailedFileNode(string filePath, Exception exception)
    {
        FullPath = filePath;
        Title = Path.GetFileName(filePath);
        Exception = exception;
    }
}