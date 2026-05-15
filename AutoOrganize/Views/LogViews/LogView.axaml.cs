using System.Collections.Specialized;
using AutoOrganize.ViewModels.LogViewModels;
using Avalonia;
using Avalonia.Controls;

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
}