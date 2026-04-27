using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Library.Services.FileTransferBatchServices;

public readonly record struct FileTransferBatchErrorInfo(
    string FilePath,
    string? OutputPath,
    MetadataBase Metadata,
    Exception Exception) : IFileTransferBatchInfo;