using AutoOrganize.ViewModels;
using Serilog.Core;
using Serilog.Events;

namespace AutoOrganize.Services;

public class LogViewSink : ILogEventSink
{
    private readonly LogViewModel _viewModel;

    public LogViewSink(LogViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void Emit(LogEvent logEvent)
    {
        _viewModel.AddLogEvent(logEvent);
    }
}