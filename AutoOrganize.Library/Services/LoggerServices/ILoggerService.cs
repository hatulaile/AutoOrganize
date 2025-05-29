using Serilog;
using Serilog.Core;

namespace AutoOrganize.Library.Services.LoggerServices;

public interface ILoggerService
{
    ILogger? ILogger { get; set; }

    LoggerConfig Config { get; }

    bool IsEnabledLogger { get; }

    bool IsWriteToFile { get; }

    bool IsWriteToView { get; }

    LoggingLevelSwitch LevelSwitch { get; }

    LoggingLevelSwitch FileLevelSwitch { get; }

    LoggingLevelSwitch ViewLevelSwitch { get; }
}