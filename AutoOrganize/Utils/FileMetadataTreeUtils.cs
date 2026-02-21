using System;
using AutoOrganize.Models.FileMetadataModels;
using AutoOrganize.Models.FileMetadataModels.FailedMetadata;

namespace AutoOrganize.Utils;

public static class FileMetadataTreeUtils
{
    public static string? IfHasSubtitleGetSubtitle(FileMetadataBase metadataBase)
    {
        if (metadataBase is ISubheading subheadingMetadataTree)
            return subheadingMetadataTree.Subheading;
        return null;
    }

    public static string? IfHasExceptionGetMessage(FileMetadataBase metadataBase)
    {
        if (metadataBase is IFailedFileSystemMetadata failedFileSystemMetadataTree)
            return failedFileSystemMetadataTree.Exception.Message;
        return null;
    }

    public static string? IfHasFullPathGetFullPath(FileMetadataBase metadataBase)
    {
        if (metadataBase is IFullPath fullPathMetadataTree)
            return fullPathMetadataTree.FullPath;
        return null;
    }

    public static string? GetTypeDisplayName(FileMetadataType type)
    {
        return type switch
        {
            FileMetadataType.None => null,
            FileMetadataType.File => "文件",
            FileMetadataType.Directory => "文件夹",
            FileMetadataType.TvSeries => "电视剧",
            FileMetadataType.TvSeason => "电视剧季",
            FileMetadataType.TvEpisode => "电视剧集",
            FileMetadataType.Movie => "电影",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}