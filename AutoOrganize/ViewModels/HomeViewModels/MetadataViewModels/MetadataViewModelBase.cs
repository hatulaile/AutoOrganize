using System;
using AutoOrganize.ViewModels.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.ViewModels.HomeViewModels.MetadataViewModels;

public partial class MetadataViewModelBase<TFileMetadata> : ViewModelBase, IMetadataViewModel<TFileMetadata>, INavigationViewModel<TFileMetadata>
{
    public virtual void OnNavigatingTo(TFileMetadata metadata)
    {
    }

    public virtual void OnNavigatedTo(TFileMetadata metadata)
    {
    }

    public void OnParametersChanged(TFileMetadata args)
    {
        Metadata = args;
    }

    public void OnNavigatedFrom()
    {
        Metadata = default;
    }

    public virtual void OnNavigatingFrom()
    {
    }

    [ObservableProperty]
    public partial TFileMetadata? Metadata { get; set; }

    partial void OnMetadataChanging(TFileMetadata? value)
    {
        MetadataChanging(value);
    }

    partial void OnMetadataChanged(TFileMetadata? value)
    {
        MetadataChanged(value);
    }

    protected virtual void MetadataChanging(TFileMetadata? value)
    {
    }

    protected virtual void MetadataChanged(TFileMetadata? value)
    {
    }


    object? IMetadataViewModel.Metadata
    {
        get => Metadata;
        set
        {
            if (value is null)
            {
                Metadata = default;
                return;
            }

            if (value is not TFileMetadata fileMetadata)
                throw new ArgumentException(
                    $"Expected value of type {typeof(TFileMetadata).FullName}, but got {value.GetType().FullName}");

            Metadata = fileMetadata;
        }
    }
}