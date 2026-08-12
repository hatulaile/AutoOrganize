using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;

public abstract class TvMetadata<TSelf> : MetadataBase<TSelf>
    where TSelf : TvMetadata<TSelf>
{
}