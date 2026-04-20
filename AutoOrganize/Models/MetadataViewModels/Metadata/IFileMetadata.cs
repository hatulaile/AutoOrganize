using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Models.MetadataViewModels.Metadata;

public interface IFileMetadata;

public interface IFileMetadata<out TMetadata> : IFileMetadata
    where TMetadata : MetadataBase
{
    TMetadata Metadata { get; }
}