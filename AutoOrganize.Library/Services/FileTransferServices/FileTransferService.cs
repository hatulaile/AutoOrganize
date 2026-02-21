using AutoOrganize.Library.Exceptions;
using AutoOrganize.Library.Models.FileTransfers;
using AutoOrganize.Library.Utils;

namespace AutoOrganize.Library.Services.FileTransferServices;

public sealed class FileTransferService : IFileTransferService
{
    public async Task TransferFileAsync(FileTransferEntry entry, FileTransferOptions options,
        CancellationToken token = default)
    {
        string? directoryName = Path.GetDirectoryName(entry.Output);
        if (string.IsNullOrEmpty(directoryName))
            throw new InvalidOutputPathException(entry.Output);
        Directory.CreateDirectory(directoryName);

        bool isExists = false;

        if (!File.Exists(entry.Input))
        {
            throw new FileNotFoundException($"File {entry.Input} not found");
        }

        if (File.Exists(entry.Output))
        {
            if (!options.CanOverwrite)
            {
                throw new IOException($"File {entry.Output} already exists and overwrite is not allowed");
            }

            isExists = true;
        }

        switch (options.Mode)
        {
            case FileTransferMode.HardLink:
                if (isExists) await Task.Run(() => File.Delete(entry.Output), token).ConfigureAwait(false);
                HardlinkUtils.CreateHardlink(entry.Output, entry.Input);
                break;
            case FileTransferMode.SymbolicLink:
                if (isExists) await Task.Run(() => File.Delete(entry.Output), token).ConfigureAwait(false);
                File.CreateSymbolicLink(entry.Output, entry.Input);
                break;
            case FileTransferMode.Copy:
                File.Copy(entry.Input, entry.Output, options.CanOverwrite);
                break;
            case FileTransferMode.Clipping:
                File.Move(entry.Input, entry.Output, options.CanOverwrite);
                break;
            case FileTransferMode.None:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}