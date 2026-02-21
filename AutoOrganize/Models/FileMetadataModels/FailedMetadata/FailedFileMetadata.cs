using System;
using System.IO;

namespace AutoOrganize.Models.FileMetadataModels.FailedMetadata;

public sealed class FailedFileMetadata : FileMetadataBase, IFullPath, IFailedFileSystemMetadata
{
    public string FullPath { get; }

    public override string? Title { get; }

    public override FileMetadataType Type => FileMetadataType.File;

    public Exception Exception { get; }

    public FailedFileMetadata(string filePath, Exception exception)
    {
        FullPath = filePath;
        Title = Path.GetFileName(filePath);
        Exception = exception;
    }
}