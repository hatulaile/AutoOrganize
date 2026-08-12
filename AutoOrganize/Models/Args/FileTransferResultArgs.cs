using System.Collections.Generic;
using AutoOrganize.Library.Services.FileTransferBatchServices;

namespace AutoOrganize.Models.Args;

public readonly struct FileTransferResultArgs
{
    public FileTransferResultArgs()
    {
    }

    public IEnumerable<IFileTransferBatchInfo>? BatchInfos { get; init; }

    public bool IsClear { get; init; } = true;
}