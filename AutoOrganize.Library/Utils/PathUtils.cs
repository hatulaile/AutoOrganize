namespace AutoOrganize.Library.Utils;

public static class PathUtils
{
    private static HashSet<char>? _invalidFileName = null;

    private static HashSet<char>? _invalidPath = null;

    public static string GetRootAppdataPath() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public static string GetDefaultAppdataPath() => Path.Combine(GetRootAppdataPath(), "hatu.AutoOrganize");

    public static string GetValidFileName(string fileName, char replacement = '_')
    {
        return string.Create(fileName.Length, (fileName, replacement, _invalidFileName), (span, state) =>
        {
            for (int i = 0; i < state.fileName.Length; i++)
            {
                span[i] = IsValidFileNameCharacter(state.fileName[i]) ? state.fileName[i] : state.replacement;
            }
        });
    }

    public static string GetInvalidPath(string path, char replacement = '_')
    {
        return string.Create(path.Length, (path, replacement, _invalidPath), (span, state) =>
        {
            for (int i = 0; i < state.path.Length; i++)
            {
                span[i] = IsValidPathCharacter(state.path[i]) ? state.path[i] : state.replacement;
            }
        });
    }

    public static bool TryGetValidFileName(ReadOnlySpan<char> fileName, Span<char> span, char replaceCharacter,
        out int written)
    {
        if (fileName.Length > span.Length)
        {
            written = 0;
            return false;
        }

        for (int i = 0; i < fileName.Length; i++)
            span[i] = IsValidFileNameCharacter(fileName[i]) ? fileName[i] : replaceCharacter;

        written = fileName.Length;
        return true;
    }

    public static bool IsValidPath(ReadOnlySpan<char> path)
    {
        _invalidFileName ??= [.. Path.GetInvalidFileNameChars()];
        _invalidPath ??= [.. Path.GetInvalidPathChars()];
        foreach (char c in path)
        {
            if (_invalidPath.Contains(c))
                return false;
        }

        return true;
    }

    public static bool IsValidFileNameCharacter(char c)
    {
        _invalidFileName ??= [.. Path.GetInvalidFileNameChars()];
        return !_invalidFileName.Contains(c);
    }

    public static bool IsValidPathCharacter(char c)
    {
        _invalidPath ??= [.. Path.GetInvalidPathChars()];
        return !_invalidPath.Contains(c);
    }
}