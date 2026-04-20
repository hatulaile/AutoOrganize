using System.IO;

namespace AutoOrganize.Models.MetadataViewModels.FileSystem;

public sealed class FailedDirectoryModel : FileMetadataBase, IFullPath
{
    public override string? Title { get; }

    public override FileMetadataType Type => FileMetadataType.Directory;

    public string FullPath { get; }

    public override bool HasChildren => true;

    public FailedDirectoryModel(string directoryPath)
    {
        FullPath = directoryPath;
        Title = Path.GetFileName(directoryPath);
    }

}