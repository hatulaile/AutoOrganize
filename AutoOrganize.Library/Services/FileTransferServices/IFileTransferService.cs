using AutoOrganize.Library.Models.FileTransfers;

namespace AutoOrganize.Library.Services.FileTransferServices;

public interface IFileTransferService
{
    Task TransferFileAsync(FileTransferEntry entry, FileTransferOptions options, CancellationToken token = default);
}