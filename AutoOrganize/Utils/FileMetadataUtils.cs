using System;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Utils;

public static class FileMetadataUtils
{
    public static string? IfHasSubtitleGetSubtitle(IMetadataTreeNode? metadataBase)
    {
        if (metadataBase is ISubheading subheadingMetadataTree)
            return subheadingMetadataTree.Subheading;
        return null;
    }

    public static string? IfHasExceptionGetMessage(IMetadataTreeNode? metadataBase)
    {
        if (metadataBase is IFailedFile failedFileSystemMetadataTree)
            return failedFileSystemMetadataTree.Exception.Message;
        return null;
    }

    public static string? IfHasFullPathGetFullPath(IMetadataTreeNode? metadataBase)
    {
        if (metadataBase is IFileSystemNode fullPathMetadataTree)
            return fullPathMetadataTree.FullPath;
        return null;
    }

    public static string? GetTypeDisplayName(MetadataNodeType nodeType)
    {
        return nodeType switch
        {
            MetadataNodeType.None => null,
            MetadataNodeType.File => "文件",
            MetadataNodeType.Directory => "文件夹",
            MetadataNodeType.TvSeries => "电视剧",
            MetadataNodeType.TvSeason => "电视剧季",
            MetadataNodeType.TvEpisode => "电视剧集",
            MetadataNodeType.Movie => "电影",
            _ => throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null)
        };
    }
}