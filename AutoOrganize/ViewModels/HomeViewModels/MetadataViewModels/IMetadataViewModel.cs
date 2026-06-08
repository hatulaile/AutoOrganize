namespace AutoOrganize.ViewModels.HomeViewModels.MetadataViewModels;

public interface IMetadataViewModel
{
    object? Metadata { get; set; }
}

public interface IMetadataViewModel<TFileMetadata> : IMetadataViewModel
{
    new TFileMetadata? Metadata { get; set; }
}