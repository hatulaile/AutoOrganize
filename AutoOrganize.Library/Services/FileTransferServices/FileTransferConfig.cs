using System.Text.Json.Serialization;
using AutoOrganize.Library.Models.FileTransfers;
using AutoOrganize.Library.Services.Config;

namespace AutoOrganize.Library.Services.FileTransferServices;

public sealed class FileTransferConfig : IConfig<FileTransferConfig>
{
    [JsonConverter(typeof(JsonStringEnumConverter<FileTransferMode>))]
    public FileTransferMode Mode { get; set; } = FileTransferMode.Copy;

    public bool IsCreateMovieFolder { get; set; } = true;

    public string OutputDirectory { get; set; } = "./Organize";

    public bool CanOverwrite { get; set; }

    public static void Copy(FileTransferConfig target, FileTransferConfig source)
    {
        target.Mode = source.Mode;
        target.OutputDirectory = source.OutputDirectory;
        target.CanOverwrite = source.CanOverwrite;
    }
}