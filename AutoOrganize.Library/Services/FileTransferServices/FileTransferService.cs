using AutoOrganize.Library.Exceptions;
using AutoOrganize.Library.Models.FileTransfers;
using AutoOrganize.Library.Utils;
using Microsoft.Extensions.Logging;

namespace AutoOrganize.Library.Services.FileTransferServices;

public sealed class FileTransferService : IFileTransferService
{
    private readonly ILogger<FileTransferService> _logger;

    public async Task TransferFileAsync(FileTransferEntry entry, FileTransferOptions options,
        CancellationToken token = default)
    {
        _logger.LogDebug("开始传输文件: {Input} -> {Output}, 模式: {Mode}",
            entry.Input, entry.Output, options.Mode);
        string? directoryName = Path.GetDirectoryName(entry.Output);
        if (string.IsNullOrEmpty(directoryName))
            throw new InvalidOutputPathException(entry.Output);

        if (!Directory.Exists(directoryName))
        {
            _logger.LogDebug("创建目标目录: {Directory}", directoryName);
            Directory.CreateDirectory(directoryName);
        }

        bool isExists = false;

        if (!File.Exists(entry.Input))
        {
            _logger.LogError("源文件不存在: {Input}", entry.Input);
            throw new FileNotFoundException($"File {entry.Input} not found");
        }

        if (File.Exists(entry.Output))
        {
            if (!options.CanOverwrite)
            {
                _logger.LogWarning("目标文件已存在且不允许覆盖: {Output}", entry.Output);
                throw new IOException($"File {entry.Output} already exists and overwrite is not allowed");
            }

            isExists = true;
            _logger.LogDebug("目标文件已存在，将被覆盖: {Output}", entry.Output);
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
                _logger.LogError("无效的文件传输模式: {Mode}", options.Mode);
                throw new ArgumentOutOfRangeException(
                    nameof(options),
                    options.Mode,
                    null);
        }

        _logger.LogInformation("文件传输完成: {Input} -> {Output} (模式: {Mode})",
            entry.Input, entry.Output, options.Mode);
    }

    public FileTransferService(ILogger<FileTransferService> logger)
    {
        _logger = logger;
    }
}