using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using System.Threading.Tasks;
using AutoOrganize.Library.Collections;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.LoggerServices;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.TopLevelServices;
using CommunityToolkit.Mvvm.Input;
using Serilog.Events;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.LogViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton, ViewModelLifetime.Singleton)]
public sealed partial class LogViewModel : ViewModelBase, INavigationViewModel
{
    private readonly IClipboardServices _clipboardServices;

    private readonly Channel<LogEvent> _pendingLogEvent = Channel.CreateUnbounded<LogEvent>(new UnboundedChannelOptions
    {
        AllowSynchronousContinuations = true,
        SingleReader = true,
    });

    private readonly LoggerConfig _config;

    public ObservableCircularBuffer<LogEvent> LogEvents { get; }

    public LogViewModel(IFileConfigManager fileConfigManager, IClipboardServices clipboardServices)
    {
        _clipboardServices = clipboardServices;
        _config = fileConfigManager.GetRequiredConfig<LoggerConfig>();
        LogEvents = new ObservableCircularBuffer<LogEvent>(_config.ViewMaxLogEntries);
        _config.ViewMaxLogEntriesChanged += ev => LogEvents.Capacity = ev.NewValue;
        _ = StartProcessingAsync();
    }

    public void AddLogEvent(LogEvent logEvent)
    {
        if (logEvent.Level < _config.ViewLogLevel)
            return;
        _pendingLogEvent.Writer.TryWrite(logEvent);
    }

    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    private async Task StartProcessingAsync()
    {
        while (true)
        {
            try
            {
                await foreach (var logEvent in _pendingLogEvent.Reader.ReadAllAsync())
                {
                    if (logEvent.Level < _config.ViewLogLevel)
                        continue;

                    LogEvents.Put(logEvent);
                }
            }
            catch (Exception)
            {
                await Task.Delay(1000);
            }
        }
    }

    [RelayCommand]
    private async Task CopyString(string text)
    {
        await _clipboardServices.SetTextAsync(text);
    }
}