using System;

namespace AutoOrganize.Models.MetadataNodes.Abstractions;

public interface IFailedFile : IFailedNode, IFileSystemNode
{
    Exception Exception { get; }
}