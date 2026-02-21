namespace AutoOrganize.ViewModels.FileMetadataViewModels;

public interface IFileMetadataViewModel
{
    object? Metadata { get; set; }
}

public interface IFileMetadataViewModel<TFileMetadata> : IFileMetadataViewModel
{
    new TFileMetadata? Metadata { get; set; }
}