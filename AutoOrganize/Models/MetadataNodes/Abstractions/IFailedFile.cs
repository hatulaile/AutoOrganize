using System;

namespace AutoOrganize.Models.MetadataNodes.Abstractions;

public interface IFailedFile
{
    Exception Exception { get; }
}