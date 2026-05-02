using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace AutoOrganize.Library.Services.LoggerServices;

public interface ILoggerService
{
    ILogger? ILogger { get; set; }

    bool IsEnabledLogger { get; }

    bool IsWriteToFile { get; }

    bool IsWriteToView { get; }

    LoggerConfig Config { get; }

    LoggingLevelSwitch LevelSwitch { get; }

    void SetLogLevel(LogEventLevel logLevel);

    Task SetLogLevelAsync(LogEventLevel logLevel);
}