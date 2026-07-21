using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

namespace AutoOrganize.Library.Models;

public sealed record FileMetadataEntry(string FilePath, MetadataBase Metadata);