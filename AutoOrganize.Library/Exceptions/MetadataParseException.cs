namespace AutoOrganize.Library.Exceptions;

public sealed class MetadataParseException : Exception
{
    public string FilePath { get; }
    public string MetadataType { get; }

    public MetadataParseException(string filePath, string metadataType)
        : base($"Failed to parse '{filePath}' as {metadataType} metadata.")
    {
        FilePath = filePath;
        MetadataType = metadataType;
    }

    public MetadataParseException(string filePath, string metadataType, string message)
        : base(message)
    {
        FilePath = filePath;
        MetadataType = metadataType;
    }
}