using System.IO;

namespace AutoOrganize.Models.MetadataViewModels.FileSystem;

public sealed class TransferFileModel : FileMetadataBase, IFullPath, ISubheading
{
    public override string? Title { get; }

    public override FileMetadataType Type => FileMetadataType.File;

    public string Subheading { get; }

    public string FullPath { get; }

    public string OutputPath { get; }

    public TransferFileModel(string filePath, string outputPath)
    {
        FullPath = filePath;
        Subheading = Path.GetExtension(filePath);
        Title = Path.GetFileNameWithoutExtension(filePath);
        OutputPath = outputPath;
    }
}