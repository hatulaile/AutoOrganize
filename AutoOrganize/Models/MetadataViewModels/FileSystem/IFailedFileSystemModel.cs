using System;

namespace AutoOrganize.Models.MetadataViewModels.FileSystem;

public interface IFailedFileSystemModel
{
    Exception Exception { get; }
}