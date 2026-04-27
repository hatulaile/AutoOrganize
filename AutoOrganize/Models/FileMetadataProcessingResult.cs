using System;
using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Models;

public readonly struct FileMetadataProcessingResult
{
    public string FilePath { get; }

    public MetadataBase? Metadata { get; }

    public Exception? Error { get; }

    [MemberNotNullWhen(true, nameof(Metadata))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public FileMetadataProcessingResult(string filePath, Exception? error)
    {
        FilePath = filePath;
        Error = error;
        IsSuccess = false;
    }

    public FileMetadataProcessingResult(string filePath, MetadataBase metadata)
    {
        FilePath = filePath;
        Metadata = metadata;
        IsSuccess = true;
    }
}