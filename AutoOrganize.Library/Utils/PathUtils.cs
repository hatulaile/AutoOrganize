namespace AutoOrganize.Library.Utils;

public static class PathUtils
{
    private static HashSet<char>? _invalidFileName = null;

    private static HashSet<char>? _invalidPath = null;

    public static string GetRootAppdataPath() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public static string GetDefaultAppdataPath() => Path.Combine(GetRootAppdataPath(), "hatu.AutoOrganize");

    public static string GetValidFileName(string fileName, char replacement = '_')
    {
        _invalidFileName ??= [..Path.GetInvalidFileNameChars()];
        return string.Create(fileName.Length, (fileName, replacement, _invalidFileName), (span, state) =>
        {
            for (int i = 0; i < state.fileName.Length; i++)
            {
                span[i] = state._invalidFileName.Contains(state.fileName[i]) ? state.replacement : state.fileName[i];
            }
        });
    }

    public static string GetInvalidPath(string path, char replacement = '_')
    {
        _invalidPath ??= [..Path.GetInvalidPathChars()];
        return string.Create(path.Length, (path, replacement, _invalidPath), (span, state) =>
        {
            for (int i = 0; i < state.path.Length; i++)
            {
                span[i] = state._invalidPath.Contains(state.path[i]) ? state.replacement : state.path[i];
            }
        });
    }

    public static bool IsValidPath(ReadOnlySpan<char> path)
    {
        _invalidFileName ??= [..Path.GetInvalidFileNameChars()];
        _invalidPath ??= [..Path.GetInvalidPathChars()];
        foreach (char c in path)
        {
            if(_invalidPath.Contains(c))
                return false;
        }

        return true;
    }
}