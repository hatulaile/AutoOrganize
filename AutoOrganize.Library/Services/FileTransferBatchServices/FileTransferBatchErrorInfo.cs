using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Library.Services.FileTransferBatchServices;

public record FileTransferBatchErrorInfo(
    string FilePath,
    string? OutputPath,
    MetadataBase Metadata,
    Exception Exception) : IFileTransferBatchInfo;