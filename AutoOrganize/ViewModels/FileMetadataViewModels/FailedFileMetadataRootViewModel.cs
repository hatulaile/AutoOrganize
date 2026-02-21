using System.Collections.Specialized;
using System.Linq;
using AutoOrganize.Models.FileMetadataModels;
using AutoOrganize.Models.FileMetadataModels.FailedMetadata;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.ViewModels.FileMetadataViewModels;

public sealed partial class FailedFileMetadataRootViewModel : MetadataViewModelBase<FailedFileMetadataRoot>
{
    [ObservableProperty] private long? _errorCount;

    protected override void MetadataChanging(FailedFileMetadataRoot? value)
    {
        base.MetadataChanging(value);
        if (value is not null)
        {
            RegisteredEvent(value);
            ErrorCount = CountErrors(value);
        }
    }

    protected override void MetadataChanged(FailedFileMetadataRoot? value)
    {
        base.MetadataChanged(value);
    }

    private static long CountErrors(FileMetadataBase metadata)
    {
        long count = 0;
        foreach (FileMetadataBase fileMetadataBase in metadata.Children)
        {
            if (fileMetadataBase is FailedFileMetadata)
                count++;

            if (!fileMetadataBase.HasChildren) continue;
            count += CountErrors(fileMetadataBase);
        }

        return count;
    }

    private void RegisteredEvent(FileMetadataBase children)
    {
        if (!children.HasChildren)
            return;
        children.Children.CollectionChanged += OnChildrenChanged;

        foreach (var item in children.Children)
            RegisteredEvent(item);
    }

    private void UnregisteredEvent(FileMetadataBase children)
    {
        if (!children.HasChildren)
            return;
        children.Children.CollectionChanged -= OnChildrenChanged;

        foreach (var item in children.Children)
            UnregisteredEvent(item);
    }

    private void OnChildrenChanged(object? o, NotifyCollectionChangedEventArgs args)
    {
        if (args.NewItems is not null)
        {
            foreach (var item in args.NewItems.Cast<FileMetadataBase>())
            {
                RegisteredEvent(item);
            }
        }

        if (args.OldItems is not null)
        {
            foreach (var item in args.OldItems.Cast<FileMetadataBase>())
            {
                UnregisteredEvent(item);
            }
        }

        ErrorCount = CountErrors(Metadata!);
    }
}