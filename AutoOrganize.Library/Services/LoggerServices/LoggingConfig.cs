using System.Text.Json.Serialization;
using AutoOrganize.Library.Services.Config;
using Serilog.Events;

namespace AutoOrganize.Library.Services.LoggerServices;

public sealed class LoggerConfig : IConfig<LoggerConfig>
{
    [JsonConverter(typeof(JsonStringEnumConverter<LogEventLevel>))]
    public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

    public bool IsEnabledLogger { get; set; } = true;

    public bool IsWriteToFile { get; set; } = true;

    public bool IsWriteToView { get; set; } = true;

    public static void Copy(LoggerConfig target, LoggerConfig source)
    {
        target.LogLevel = source.LogLevel;
        target.IsWriteToFile = source.IsWriteToFile;
    }
}