using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

namespace AutoOrganize.Models.MetadataNodes.Abstractions;

public interface IFileMetadata
{
    MetadataBase Metadata { get; }
}

public interface IFileMetadata<out TMetadata> : IFileMetadata
    where TMetadata : MetadataBase
{
    MetadataBase IFileMetadata.Metadata => Metadata;

    new TMetadata Metadata { get; }
}