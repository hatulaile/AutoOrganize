namespace AutoOrganize.ViewModels.HomeViewModels.MetadataViewModels;

public interface IFileMetadataViewModel
{
    object? Metadata { get; set; }
}

public interface IMetadataViewModel<TFileMetadata> : IFileMetadataViewModel
{
    new TFileMetadata? Metadata { get; set; }
}