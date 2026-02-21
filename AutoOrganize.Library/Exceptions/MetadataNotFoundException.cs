namespace AutoOrganize.Library.Exceptions;

public sealed class MetadataNotFoundException : Exception
{
    public string FilePath { get; }
    public string MetadataType { get; }

    public MetadataNotFoundException(string filePath, string metadataType)
        : base($"No matching {metadataType} metadata found for '{filePath}'.")
    {
        FilePath = filePath;
        MetadataType = metadataType;
    }

    public MetadataNotFoundException(string filePath, string metadataType, string message)
        : base(message)
    {
        FilePath = filePath;
        MetadataType = metadataType;
    }
}