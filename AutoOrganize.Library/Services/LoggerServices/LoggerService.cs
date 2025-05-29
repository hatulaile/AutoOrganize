using AutoOrganize.Library.Services.Config;
using Serilog;
using Serilog.Core;

namespace AutoOrganize.Library.Services.LoggerServices;

public sealed class LoggerService : ILoggerService
{
    private readonly IFileConfigManager _fileConfigManager;

    public ILogger? ILogger { get; set; }

    public bool IsEnabledLogger => Config.IsEnabledLogger;

    public bool IsWriteToFile => IsEnabledLogger && Config.IsWriteToFile;

    public bool IsWriteToView => IsEnabledLogger && Config.IsWriteToView;

    public LoggerConfig Config { get; }

    public LoggingLevelSwitch LevelSwitch { get; } = new();

    public LoggingLevelSwitch FileLevelSwitch { get; } = new();

    public LoggingLevelSwitch ViewLevelSwitch { get; } = new();

    public LoggerService(IFileConfigManager fileConfigManager)
    {
        _fileConfigManager = fileConfigManager;
        Config = fileConfigManager.GetConfigOrLoad<LoggerConfig>();
        LevelSwitch.MinimumLevel = Config.LogLevel;
        FileLevelSwitch.MinimumLevel = Config.FileLogLevel;
        ViewLevelSwitch.MinimumLevel = Config.ViewLogLevel;
        Config.LogLevelChanged += ev => LevelSwitch.MinimumLevel = ev.NewValue;
        Config.FileLogLevelChanged += ev => FileLevelSwitch.MinimumLevel = ev.NewValue;
        Config.ViewLogLevelChanged += ev => ViewLevelSwitch.MinimumLevel = ev.NewValue;
    }
}