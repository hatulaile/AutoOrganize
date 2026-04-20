namespace AutoOrganize.ViewModels.MetadataViewModels;

public interface IFileMetadataViewModel
{
    object? Metadata { get; set; }
}

public interface IMetadataViewModel<TFileMetadata> : IFileMetadataViewModel
{
    new TFileMetadata? Metadata { get; set; }
}