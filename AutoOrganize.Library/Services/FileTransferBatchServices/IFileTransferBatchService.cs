using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Observers;

namespace AutoOrganize.Library.Services.FileTransferBatchServices;

public interface IFileTransferBatchService
{
    Task<FileTransferBatchResult> ProcessFilesAsync(IEnumerable<FileMetadataEntry> fileMetadataEntries,
        IProcessObserver<FileTransferBatchInfo, FileTransferBatchResult, FileTransferBatchErrorInfo>? progress = null,
        CancellationToken token = default);
}