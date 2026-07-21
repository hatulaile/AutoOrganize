using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

namespace AutoOrganize.Library.Services.FileTransferBatchServices;

public readonly record struct FileTransferBatchInfo(string FilePath, string OutputPath, MetadataBase Metadata) : IFileTransferBatchInfo;