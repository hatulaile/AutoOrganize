using System.Collections.Generic;
using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.MetadataNodes.FileSystem;
using AutoOrganize.Models.MetadataNodes.Metadata;

namespace AutoOrganize.Models.MenuItemViewModelContext;

public sealed class MetadataEditorMenuItemContext : IMenuItemContext
{
    public required IReadOnlyList<MetadataTreeNodeBase> SelectedItems { get; init; }

    public required FailedSourceFileRoot FailedSourceFileRoot { get; init; }

    public required MetadataTreeRoot MetadataTreeRoot { get; init; }
}