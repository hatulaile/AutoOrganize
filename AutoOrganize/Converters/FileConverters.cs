using System.IO;
using Avalonia.Data.Converters;

namespace AutoOrganize.Converters;

public static class FileConverters
{
    public static FuncValueConverter<long, string?> FileLengthToReadableConverter => field ??=
        new(length => length switch
        {
            < 1024L => $"{length}B",
            < 1024L * 1024L => $"{length / 1024D:F1} KB",
            < 1024L * 1024L * 1024L => $"{length / (1024D * 1024D):F1} MB",
            < 1024L * 1024L * 1024L * 1024L => $"{length / (1024D * 1024D * 1024D):F2} GB",
            _ => $"{length / (1024D * 1024D * 1024D * 1024D):F2} TB"
        });

    public static FuncValueConverter<string, bool> FileNameToExistsConverter => field ??=
        new FuncValueConverter<string, bool>(File.Exists);

    public static FuncValueConverter<string, bool> FilePathToDirectoryExistsConverter => field ??=
        new FuncValueConverter<string, bool>(path => Directory.Exists(Path.GetDirectoryName(path)));

    public static FuncValueConverter<string, string?> FileNameToExtensionConverter => field ??=
        new FuncValueConverter<string, string?>(fileName => Path.GetExtension(fileName)?.Remove(0, 1)?.ToUpper());

    public static FuncValueConverter<string, string?> FileNameToNameWithoutExtensionConverter => field ??=
        new FuncValueConverter<string, string?>(Path.GetFileNameWithoutExtension);
}