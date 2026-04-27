using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Library.Services.FileTransferBatchServices;

public readonly record struct FileTransferBatchInfo(string FilePath, string OutputPath, MetadataBase Metadata) : IFileTransferBatchInfo;