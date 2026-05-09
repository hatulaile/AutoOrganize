using System.Collections.Generic;
using AutoOrganize.Library.Services.FileTransferBatchServices;

namespace AutoOrganize.Models.Options;

public readonly struct FileTransferResultOptions
{
    public FileTransferResultOptions()
    {
    }

    public IEnumerable<IFileTransferBatchInfo>? BatchInfos { get; init; }

    public bool IsClear { get; init; } = true;
}