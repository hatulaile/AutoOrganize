using System.IO;

namespace AutoOrganize.Models.FileMetadataModels.SuccessMetadata;

public class FileMetadata : FileMetadataBase, IFullPath, ISubheading
{
    public override string? Title { get; }

    public override FileMetadataType Type => FileMetadataType.File;

    public string FullPath { get; }

    public string Subheading { get; }

    public FileMetadata(string fullPath)
    {
        FullPath = fullPath;
        Subheading = Path.GetExtension(fullPath);
        Title = Path.GetFileNameWithoutExtension(fullPath);
    }
}