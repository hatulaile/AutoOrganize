using System;

namespace AutoOrganize.Utils;

public static class PathUtils
{
    public static bool IsSubPath(string parentPath, string childPath)
    {
        var parentUri = new Uri(parentPath);
        var childUri = new Uri(childPath);
        return parentUri.IsBaseOf(childUri);
    }

    public static bool IsSamePath(string path1, string path2)
    {
        var uri1 = new Uri(path1);
        var uri2 = new Uri(path2);
        return uri1 == uri2;
    }
}