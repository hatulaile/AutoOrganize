namespace AutoOrganize.Library.Utils;

public static class StringUtils
{
    public static string ReplaceRange(ReadOnlySpan<char> input, int startIndex, int length,
        ReadOnlySpan<char> replacement)
    {
        ReadOnlySpan<char> startSpan = input[..startIndex];
        ReadOnlySpan<char> endSpan = input[(startIndex + length)..];
        return string.Concat(startSpan, replacement, endSpan);
    }
}