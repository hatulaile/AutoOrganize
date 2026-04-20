using System.Collections.Generic;

namespace AutoOrganize.Models;

public sealed class MetadataEditOption
{
    public FileProcessOptions? FileProcessOptions { get; init; }

    public IEnumerable<FileMetadataProcessingResult>? FileProcessResultInfos { get; init; }

    public bool IsClear { get; init; } = true;
}