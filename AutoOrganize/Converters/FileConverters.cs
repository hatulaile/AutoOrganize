using System.IO;
using Avalonia.Data.Converters;

namespace AutoOrganize.Converters;

public static class FileConverters
{
    public static readonly FuncValueConverter<long, string?> FileLengthToReadableConverter =
        new(length => length switch
        {
            < 1024L => $"{length}B",
            < 1024L * 1024L => $"{length / 1024D:F1} KB",
            < 1024L * 1024L * 1024L => $"{length / (1024D * 1024D):F1} MB",
            < 1024L * 1024L * 1024L * 1024L => $"{length / (1024D * 1024D * 1024D):F2} GB",
            _ => $"{length / (1024D * 1024D * 1024D * 1024D):F2} TB"
        });

    public static readonly FuncValueConverter<string, string?> FileNameToExtensionConverter =
        new(fileName => Path.GetExtension(fileName)?.Remove(0, 1)?.ToUpper());

    public static readonly FuncValueConverter<string, string?> FileNameToNameWithoutExtensionConverter =
        new(Path.GetFileNameWithoutExtension);
}