using System.IO;

namespace AutoOrganize.Models.FileMetadataModels.FailedMetadata;

public sealed class FailedDirectoryMetadata : FileMetadataBase, IFullPath
{
    public override string? Title { get; }

    public override FileMetadataType Type => FileMetadataType.Directory;

    public string FullPath { get; }

    public override bool HasChildren => true;

    public FailedDirectoryMetadata(string directoryPath)
    {
        FullPath = directoryPath;
        Title = Path.GetFileName(directoryPath);
    }

}