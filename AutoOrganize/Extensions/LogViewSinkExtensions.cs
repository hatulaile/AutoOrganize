using AutoOrganize.Services;
using AutoOrganize.ViewModels;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;

namespace AutoOrganize.Extensions;

public static class LogViewSinkExtensions
{
    extension(LoggerSinkConfiguration configuration)
    {
        public LoggerConfiguration View(LogViewModel viewModel, LoggingLevelSwitch loggingLevelSwitch)
        {
            var sink = new LogViewSink(viewModel);
            return configuration.Sink(sink, levelSwitch: loggingLevelSwitch);
        }
    }
}