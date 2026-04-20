using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Models.MetadataViewModels.Metadata;

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