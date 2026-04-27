using System.Collections.Generic;

namespace AutoOrganize.Models;

public readonly struct MetadataEditOption
{
    public MetadataEditOption()
    {
    }

    public FileProcessOptions? FileProcessOptions { get; init; } = null;

    public IEnumerable<FileMetadataProcessingResult>? FileProcessResultInfos { get; init; } = null;

    public bool IsClear { get; init; } = true;
}