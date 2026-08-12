using System;

namespace AutoOrganize.Models.MetadataNodes.Abstractions;

public interface IFailedFile : IFailedNode
{
    Exception Exception { get; }
}