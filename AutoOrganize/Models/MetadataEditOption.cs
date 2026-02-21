using System.Collections.Generic;

namespace AutoOrganize.Models;

public sealed class MetadataEditOption
{
    public FileProcessOptions? FileProcessOptions { get; }

    public IEnumerable<FileMetadataProcessingResult>? FileProcessResultInfos { get; }

    public bool IsClear { get; set; } = true;

    public MetadataEditOption(FileProcessOptions? fileProcessOptions,
        IEnumerable<FileMetadataProcessingResult>? fileProcessResultInfos)
    {
        FileProcessOptions = fileProcessOptions;
        FileProcessResultInfos = fileProcessResultInfos;
    }
}