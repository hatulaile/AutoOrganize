using System.Collections.Specialized;
using AutoOrganize.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AutoOrganize.Views;

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
}