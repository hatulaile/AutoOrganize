using System.Text.Json.Serialization;
using AutoConfigGenerator;
using AutoOrganize.Library.Models.FileTransfers;
using AutoOrganize.Library.Services.Config;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Library.Services.FileTransferServices;

[AutoConfig]
public sealed partial class FileTransferConfig : ConfigBase<FileTransferConfig>
{
    [ObservableProperty]
    [JsonConverter(typeof(JsonStringEnumConverter<FileTransferMode>))]
    public partial FileTransferMode Mode { get; set; } = FileTransferMode.Copy;

    [ObservableProperty]
    public partial bool IsCreateMovieFolder { get; set; } = true;

    [ObservableProperty]
    public partial string OutputDirectory { get; set; } = "./Organize";

    [ObservableProperty]
    public partial bool CanOverwrite { get; set; }
}