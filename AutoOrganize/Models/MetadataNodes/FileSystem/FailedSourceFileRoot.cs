using System;
using System.IO;
using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.Options;
using AutoOrganize.Utils;

namespace AutoOrganize.Models.MetadataNodes.FileSystem;

public sealed class FailedSourceFileRoot : MetadataTreeNodeBase, IMetadataTreeRoot
{
    public override string Title => "发生错误的文件列表";

    public override MetadataNodeType NodeType => MetadataNodeType.Directory;

    public override bool HasChildren => true;

    public FailedFileNode AddOrGetFailedMetadata(string path, Exception exception)
    {
        var failedFileMetadata = new FailedFileNode(path, exception);
        AddChild(failedFileMetadata);
        return failedFileMetadata;
    }

    public FailedFileNode AddOrGetFailedMetadata(string filePath, Exception exception, FileProcessOptions options)
    {
        var failedFile = new FailedFileNode(filePath, exception);
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


            FailedDirectoryNode? directoryMetadata = AddOrGetFailedDirectoryMetadata(directoryName, options);
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

    public FailedDirectoryNode? AddOrGetFailedDirectoryMetadata(string directoryPath,
        FileProcessOptions options)
    {
        FailedDirectoryNode? failedDirectoryMetadataCache =
            GetChildren<FailedDirectoryNode, FailedDirectoryNode>(
                x => PathUtils.IsSamePath(x.FullPath, directoryPath),
                x => PathUtils.IsSubPath(x.FullPath, directoryPath));
        if (failedDirectoryMetadataCache is not null)
            return failedDirectoryMetadataCache;

        var failedDirectoryTreeMetadata = new FailedDirectoryNode(directoryPath);
        foreach (string filePath in options.FilesPaths)
        {
            if (!PathUtils.IsSamePath(filePath, directoryPath)) continue;
            AddChild(failedDirectoryTreeMetadata);
            return failedDirectoryTreeMetadata;
        }

        string? path = Path.GetDirectoryName(directoryPath);
        if (path is null || PathUtils.IsSamePath(path, directoryPath)) return null;
        FailedDirectoryNode? directoryTreeMetadata = AddOrGetFailedDirectoryMetadata(path, options);
        if (directoryTreeMetadata is null) return null;
        directoryTreeMetadata.AddChild(failedDirectoryTreeMetadata);
        return failedDirectoryTreeMetadata;
    }
}