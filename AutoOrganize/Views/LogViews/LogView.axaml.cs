using System;
using System.Collections.Specialized;
using AutoOrganize.ViewModels.LogViewModels;
using AutoOrganize.ViewModels.SettingsViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Serilog;
using Serilog.Events;

namespace AutoOrganize.Views.LogViews;

public partial class LogView : UserControl
{
    private LogViewModel LogViewModel => (LogViewModel)DataContext!;

    public LogView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        LogViewModel.LogEvents.CollectionChanged += LogEventsOnCollectionChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        LogViewModel.LogEvents.CollectionChanged -= LogEventsOnCollectionChanged;
    }

    private void LogEventsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // MainScrollViewer.ScrollToEnd();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
       Log.Logger.Verbose("Verbose log event");
       Log.Logger.Debug("Debug log event");
       Log.Logger.Information("Information log event");
       Log.Logger.Warning("Warning log event");
       Log.Logger.Error("Error log event");
       Log.Logger.Fatal("Fatal log event");
    }
}