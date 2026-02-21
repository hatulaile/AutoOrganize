namespace AutoOrganize.Library.Exceptions;

public sealed class MetadataFieldNullException : Exception
{
    public string MetadataType { get; }
    public string FieldName { get; }

    public MetadataFieldNullException(string metadataType, string fieldName)
        : base($"'{metadataType}.{fieldName}' is null.")
    {
        MetadataType = metadataType;
        FieldName = fieldName;
    }

    public MetadataFieldNullException(string metadataType, string fieldName, string message)
        : base(message)
    {
        MetadataType = metadataType;
        FieldName = fieldName;
    }
}