using System;

namespace AutoOrganize.Models.MetadataViewModels;

public interface IFailedFile
{
    Exception Exception { get; }
}