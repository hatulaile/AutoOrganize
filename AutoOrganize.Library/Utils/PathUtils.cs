namespace AutoOrganize.Library.Utils;

public static class PathUtils
{
    private static HashSet<char>? _invalidFileName = null;

    private static HashSet<char>? _invalidPath = null;

    public static string GetAppdataPath() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public static string GetValidFileName(string fileName, char replacement = '_')
    {
        _invalidFileName ??= Path.GetInvalidFileNameChars().ToHashSet();
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
        _invalidPath ??= Path.GetInvalidPathChars().ToHashSet();
        return string.Create(path.Length, (path, replacement, _invalidPath), (span, state) =>
        {
            for (int i = 0; i < state.path.Length; i++)
            {
                span[i] = state._invalidPath.Contains(state.path[i]) ? state.replacement : state.path[i];
            }
        });
    }
}