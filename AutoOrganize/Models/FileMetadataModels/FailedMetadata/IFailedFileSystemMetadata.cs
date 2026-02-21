using System;

namespace AutoOrganize.Models.FileMetadataModels.FailedMetadata;

public interface IFailedFileSystemMetadata
{
    Exception Exception { get; }
}