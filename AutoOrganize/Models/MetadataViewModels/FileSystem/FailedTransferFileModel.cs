using System;
using System.IO;

namespace AutoOrganize.Models.MetadataViewModels.FileSystem;

public sealed class FailedTransferFileModel : FileMetadataBase, IFullPath, ISubheading, IFailedFile
{
    public string FullPath { get; }

    public override string? Title { get; }

    public override FileMetadataType Type => FileMetadataType.File;

    public string Subheading { get; }

    public string? OutputPath { get; }

    public Exception Exception { get; }

    public FailedTransferFileModel(string filePath, string? outputPath, Exception exception)
    {
        FullPath = filePath;
        Subheading = Path.GetExtension(filePath);
        Title = Path.GetFileNameWithoutExtension(filePath);
        OutputPath = outputPath;
        Exception = exception;
    }
}