using System;
using System.IO;
using AutoOrganize.Models.MetadataViewModels.FileSystem;
using AutoOrganize.Utils;

namespace AutoOrganize.Models.MetadataViewModels.Metadata;

public sealed class FailedMetadataRoot : FileMetadataBase, IMetadataTreeRoot
{
    public override string Title => "发生错误的文件列表";

    public override FileMetadataType Type => FileMetadataType.Directory;

    public override bool HasChildren => true;

    public FailedFileModel AddOrGetFailedMetadata(string path, Exception exception)
    {
        var failedFileMetadata = new FailedFileModel(path, exception);
        AddChild(failedFileMetadata);
        return failedFileMetadata;
    }

    public FailedFileModel AddOrGetFailedMetadata(string filePath, Exception exception, FileProcessOptions options)
    {
        var failedFile = new FailedFileModel(filePath, exception);
        foreach (string path in options.FilesPaths)
        {
            if (!PathUtils.IsSubPath(path, filePath))
                continue;

            if (PathUtils.IsSamePath(path, filePath))
            {
                AddChild(failedFile);
                return failedFile;
            }

            string? directoryName = Path.GetDirectoryName(filePath);
            if (directoryName is null)
            {
                AddChild(failedFile);
                return failedFile;
            }


            FailedDirectoryModel? directoryMetadata = AddOrGetFailedDirectoryMetadata(directoryName, options);
            if (directoryMetadata is null)
            {
                AddChild(failedFile);
                return failedFile;
            }

            directoryMetadata.AddChild(failedFile);
            return failedFile;
        }

        AddChild(failedFile);
        return failedFile;
    }

    public FailedDirectoryModel? AddOrGetFailedDirectoryMetadata(string directoryPath,
        FileProcessOptions options)
    {
        FailedDirectoryModel? failedDirectoryMetadataCache =
            GetChildren<FailedDirectoryModel, FailedDirectoryModel>(
                x => PathUtils.IsSamePath(x.FullPath, directoryPath),
                x => PathUtils.IsSubPath(x.FullPath, directoryPath));
        if (failedDirectoryMetadataCache is not null)
            return failedDirectoryMetadataCache;

        var failedDirectoryTreeMetadata = new FailedDirectoryModel(directoryPath);
        foreach (string filePath in options.FilesPaths)
        {
            if (!PathUtils.IsSamePath(filePath, directoryPath)) continue;
            AddChild(failedDirectoryTreeMetadata);
            return failedDirectoryTreeMetadata;
        }

        string? path = Path.GetDirectoryName(directoryPath);
        if (path is null || PathUtils.IsSamePath(path, directoryPath)) return null;
        FailedDirectoryModel? directoryTreeMetadata = AddOrGetFailedDirectoryMetadata(path, options);
        if (directoryTreeMetadata is null) return null;
        directoryTreeMetadata.AddChild(failedDirectoryTreeMetadata);
        return failedDirectoryTreeMetadata;
    }
}