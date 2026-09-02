using System.Collections.Generic;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Models.MenuItemViewModelContext;

public sealed class FileTransferResultMenuItemContext : IMenuItemContext
{
    public required IReadOnlyList<MetadataTreeNodeBase> SelectedItems { get; init; }
}