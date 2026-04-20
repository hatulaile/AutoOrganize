using System.IO;

namespace AutoOrganize.Models.MetadataViewModels.FileSystem;

public class FileModel : FileMetadataBase, IFullPath, ISubheading
{
    public override string? Title { get; }

    public override FileMetadataType Type => FileMetadataType.File;

    public string FullPath { get; }

    public string Subheading { get; }

    public FileModel(string fullPath)
    {
        FullPath = fullPath;
        Subheading = Path.GetExtension(fullPath);
        Title = Path.GetFileNameWithoutExtension(fullPath);
    }
}