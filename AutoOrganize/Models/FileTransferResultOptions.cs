using System.Collections.Generic;
using AutoOrganize.Library.Services.FileTransferBatchServices;

namespace AutoOrganize.Models;

public class FileTransferResultOptions
{
    public IEnumerable<IFileTransferBatchInfo>? BatchInfos { get; init; }

    public bool IsClear { get; init; } = true;
}