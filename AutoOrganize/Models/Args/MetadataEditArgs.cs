using System.Collections.Generic;

namespace AutoOrganize.Models.Args;

public readonly struct MetadataEditArgs
{
    public MetadataEditArgs()
    {
    }

    public FileProcessArgs? FileProcessArgs { get; init; } = null;

    public IEnumerable<FileMetadataProcessingResult>? FileProcessResultInfos { get; init; } = null;

    public bool IsClear { get; init; } = true;
}