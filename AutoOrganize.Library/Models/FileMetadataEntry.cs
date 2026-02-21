using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Library.Models;

public sealed record FileMetadataEntry(string FilePath, MetadataBase Metadata);