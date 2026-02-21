using System.Collections.ObjectModel;
using System.IO;

namespace AutoOrganize.Utils;

public static class VideoUtils
{
    private static readonly ReadOnlySet<string> VideoExtensions = [".mp4", ".mkv", ".avi", ".mov", ".flv", ".wmv"];

    public static bool IsVideoFile(string filePath)
    {
        return !string.IsNullOrEmpty(filePath) &&
               VideoExtensions.Contains(Path.GetExtension(filePath).ToLowerInvariant());
    }
}