using System.Collections.Specialized;
using System.Linq;
using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.MetadataNodes.FileSystem;
using CommunityToolkit.Mvvm.ComponentModel;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.HomeViewModels.MetadataViewModels;

[ViewModelRegistration]
public sealed partial class FailedFileRootViewModel : MetadataViewModelBase<FailedSourceFileRoot>
{
    [ObservableProperty]
    public partial long? ErrorCount { get; set; }

    protected override void MetadataChanging(FailedSourceFileRoot? value)
    {
        base.MetadataChanging(value);
        if (value is not null)
        {
            RegisteredEvent(value);
            ErrorCount = CountErrors(value);
        }
    }

    protected override void MetadataChanged(FailedSourceFileRoot? value)
    {
        base.MetadataChanged(value);
    }

    private static long CountErrors(MetadataTreeNodeBase metadataTreeNode)
    {
        long count = 0;
        foreach (MetadataTreeNodeBase fileMetadataBase in metadataTreeNode.Children)
        {
            if (fileMetadataBase is FailedFileNode)
                count++;

            if (!fileMetadataBase.HasChildren) continue;
            count += CountErrors(fileMetadataBase);
        }

        return count;
    }

    private void RegisteredEvent(MetadataTreeNodeBase children)
    {
        if (!children.HasChildren)
            return;
        children.Children.CollectionChanged += OnChildrenChanged;

        foreach (var item in children.Children)
            RegisteredEvent(item);
    }

    private void UnregisteredEvent(MetadataTreeNodeBase children)
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
            foreach (var item in args.NewItems.Cast<MetadataTreeNodeBase>())
            {
                RegisteredEvent(item);
            }
        }

        if (args.OldItems is not null)
        {
            foreach (var item in args.OldItems.Cast<MetadataTreeNodeBase>())
            {
                UnregisteredEvent(item);
            }
        }

        ErrorCount = CountErrors(Metadata!);
    }
}