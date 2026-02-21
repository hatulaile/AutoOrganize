using System;
using System.IO;
using AutoOrganize.Utils;

namespace AutoOrganize.Models.FileMetadataModels.FailedMetadata;

public sealed class FailedFileMetadataRoot : FileMetadataBase, IMetadataTreeRoot
{
    public override string Title => "发生错误的文件列表";

    public override FileMetadataType Type => FileMetadataType.Directory;

    public override bool HasChildren => true;

    public FailedFileMetadata AddOrGetFailedMetadata(string path, Exception exception)
    {
        var failedFileMetadata = new FailedFileMetadata(path, exception);
        AddChild(failedFileMetadata);
        return failedFileMetadata;
    }

    public FailedFileMetadata AddOrGetFailedMetadata(FileMetadataProcessingResult processingResult, FileProcessOptions options)
    {
        var failedFileTreeMetadata = new FailedFileMetadata(processingResult.FilePath, processingResult.Error!);
        foreach (string path in options.FilesPaths)
        {
            if (!PathUtils.IsSubPath(path, processingResult.FilePath))
                continue;

            if (PathUtils.IsSamePath(path, processingResult.FilePath))
            {
                AddChild(failedFileTreeMetadata);
                return failedFileTreeMetadata;
            }

            string? directoryName = Path.GetDirectoryName(processingResult.FilePath);
            if (directoryName is null)
            {
                AddChild(failedFileTreeMetadata);
                return failedFileTreeMetadata;
            }


            FailedDirectoryMetadata? directoryMetadata = AddOrGetFailedDirectoryMetadata(directoryName, options);
            if (directoryMetadata is null)
            {
                AddChild(failedFileTreeMetadata);
                return failedFileTreeMetadata;
            }

            directoryMetadata.AddChild(failedFileTreeMetadata);
            return failedFileTreeMetadata;
        }

        AddChild(failedFileTreeMetadata);
        return failedFileTreeMetadata;
    }

    public FailedDirectoryMetadata? AddOrGetFailedDirectoryMetadata(string directoryPath,
        FileProcessOptions options)
    {
        FailedDirectoryMetadata? failedDirectoryMetadataCache =
            GetChildren<FailedDirectoryMetadata, FailedDirectoryMetadata>(
                x => PathUtils.IsSamePath(x.FullPath, directoryPath),
                x => PathUtils.IsSubPath(x.FullPath, directoryPath));
        if (failedDirectoryMetadataCache is not null)
            return failedDirectoryMetadataCache;

        var failedDirectoryTreeMetadata = new FailedDirectoryMetadata(directoryPath);
        foreach (string filePath in options.FilesPaths)
        {
            if (!PathUtils.IsSamePath(filePath, directoryPath)) continue;
            AddChild(failedDirectoryTreeMetadata);
            return failedDirectoryTreeMetadata;
        }

        string? path = Path.GetDirectoryName(directoryPath);
        if (path is null || PathUtils.IsSamePath(path, directoryPath)) return null;
        FailedDirectoryMetadata? directoryTreeMetadata = AddOrGetFailedDirectoryMetadata(path, options);
        if (directoryTreeMetadata is null) return null;
        directoryTreeMetadata.AddChild(failedDirectoryTreeMetadata);
        return failedDirectoryTreeMetadata;
    }
}