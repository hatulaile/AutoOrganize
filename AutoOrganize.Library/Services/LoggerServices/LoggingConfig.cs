using System.Text.Json.Serialization;
using AutoConfigGenerator;
using AutoOrganize.Library.Services.Config;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog.Events;

namespace AutoOrganize.Library.Services.LoggerServices;

[AutoConfig]
public sealed partial class LoggerConfig : ConfigBase<LoggerConfig>
{
    [ObservableProperty]
    [JsonConverter(typeof(JsonStringEnumConverter<LogEventLevel>))]
    public partial LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

    [ObservableProperty]
    public partial bool IsEnabledLogger { get; set; } = true;

    [ObservableProperty]
    public partial bool IsWriteToFile { get; set; } = true;

    [ObservableProperty]
    public partial bool IsWriteToView { get; set; } = true;
}