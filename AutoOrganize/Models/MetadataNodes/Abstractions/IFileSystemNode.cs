namespace AutoOrganize.Models.MetadataNodes.Abstractions;

public interface IFileSystemNode : IMetadataTreeNode
{
    string FullPath { get; }
}