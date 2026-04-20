using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Library.Services.FileTransferBatchServices;

public record FileTransferBatchInfo(string FilePath, string OutputPath, MetadataBase Metadata) : IFileTransferBatchInfo;