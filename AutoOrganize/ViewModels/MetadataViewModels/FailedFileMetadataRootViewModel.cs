using System.Collections.Specialized;
using System.Linq;
using AutoOrganize.Models.MetadataViewModels;
using AutoOrganize.Models.MetadataViewModels.FileSystem;
using AutoOrganize.Models.MetadataViewModels.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.MetadataViewModels;

[ViewModelRegistration]
public sealed partial class FailedFileMetadataRootViewModel : MetadataViewModelBase<FailedMetadataRoot>
{
    [ObservableProperty] private long? _errorCount;

    protected override void MetadataChanging(FailedMetadataRoot? value)
    {
        base.MetadataChanging(value);
        if (value is not null)
        {
            RegisteredEvent(value);
            ErrorCount = CountErrors(value);
        }
    }

    protected override void MetadataChanged(FailedMetadataRoot? value)
    {
        base.MetadataChanged(value);
    }

    private static long CountErrors(FileMetadataBase metadata)
    {
        long count = 0;
        foreach (FileMetadataBase fileMetadataBase in metadata.Children)
        {
            if (fileMetadataBase is FailedFileModel)
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