using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.FileTransferBatchServices;
using AutoOrganize.Library.Services.Observers;
using AutoOrganize.Models.Args;
using AutoOrganize.Models.MenuItemViewModelContext;
using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.MetadataNodes.FileSystem;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Nito.Disposables.Internals;

namespace AutoOrganize.ViewModels.HomeViewModels;

public partial class FileTransferResultViewModel
{
    public IReadOnlyList<IMenuEntry> MenuItemViewModels => field ??= CreateMenuItems();

    private IReadOnlyList<IMenuEntry> CreateMenuItems() =>
    [
        new MenuItem<FileTransferResultMenuItemContext>("重试失败", RetryFailedCommand,
            static context => HasSelectedFailedFiles(context)),
        new MenuItem<FileTransferResultMenuItemContext>("重试全部", RetryAllCommand,
            static context => HasSelectedFiles(context)),

        new MenuSeparator(static context => context is FileTransferResultMenuItemContext { SelectedItems.Count: > 0 }),
        new MenuItem<FileTransferResultMenuItemContext>("删除", RemoveTransferItemCommand,
            static context => context.SelectedItems.Count > 0),
    ];

    private static bool HasSelectedFailedFiles(FileTransferResultMenuItemContext context) =>
        context.SelectedItems.Any(x => x is FailedTransferFileNode) ||
        context.SelectedItems.Any(x => x.HasChildren && x.FindChildren<FailedTransferFileNode>().Any());

    private static bool HasSelectedFiles(FileTransferResultMenuItemContext context) =>
        context.SelectedItems.Any(static x => x is TransferredFileNode or FailedTransferFileNode) ||
        context.SelectedItems.Any(static x =>
            x.HasChildren &&
            x.FindChildren<MetadataTreeNodeBase>(static n => n is TransferredFileNode or FailedTransferFileNode).Any());

    [RelayCommand]
    private async Task RetryFailedAsync(FileTransferResultMenuItemContext? context)
    {
        if (context is null)
            return;

        await RetryFilesAsyncInternal(CollectSelectedNodesFailedOnly(context.SelectedItems));
    }

    [RelayCommand]
    private async Task RetryAllAsync(FileTransferResultMenuItemContext? context)
    {
        if (context is null)
            return;

        await RetryFilesAsyncInternal(CollectSelectedNodes(context.SelectedItems));
    }

    [RelayCommand]
    private async Task RemoveTransferItemAsync(FileTransferResultMenuItemContext? context)
    {
        if (context is null || context.SelectedItems.Count == 0 || _metadataRoot is null)
            return;

        try
        {
            List<MetadataTreeNodeBase> selectedItems = CollectSelectedNodes(context.SelectedItems);

            int failedCount = 0;
            int successCount = 0;
            foreach (MetadataTreeNodeBase node in selectedItems)
            {
                successCount += node.FindChildren<TransferredFileNode>().Count();
                failedCount += node.FindChildren<FailedTransferFileNode>().Count();
            }

            RemoveTransferConfirmResult result = await _windowService.ShowDialog
                <RemoveTransferConfirmWindowViewModel, RemoveTransferConfirmArgs, RemoveTransferConfirmResult>
                (new RemoveTransferConfirmArgs(successCount, failedCount), this);

            if (!result.IsConfirmed)
                return;

            if (result.IsDeleteFilesOnDisk)
                await Task.Run(() => DeleteOutputFilesOnDisk(selectedItems));

            foreach (MetadataTreeNodeBase node in selectedItems)
                _metadataRoot.RemoveChildAndEmptyParent(node, false);

            _notificationServices.Show(new Notification("删除完成",
                $"已移除 {successCount + failedCount} 个条目", NotificationType.Success), this);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "删除传输条目异常");
        }

        return;

        Task DeleteOutputFilesOnDisk(IReadOnlyList<MetadataTreeNodeBase> selectedItems)
        {
            try
            {
                foreach (var node in selectedItems)
                {
                    if (node is TransferredFileNode transferredFile)
                    {
                        TryDeleteFile(transferredFile.FullPath);
                        continue;
                    }

                    if (!node.HasChildren)
                        continue;

                    foreach (TransferredFileNode transferredFileNode in node.FindChildren<TransferredFileNode>())
                        TryDeleteFile(transferredFileNode.FullPath);
                }
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }

            return Task.CompletedTask;

            void TryDeleteFile(string path)
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "删除磁盘文件失败: {Path}", path);
                }
            }
        }
    }

    private async Task RetryFilesAsyncInternal(IReadOnlyList<MetadataTreeNodeBase> nodes)
    {
        if (_metadataRoot is null)
            return;

        try
        {
            var observer =
                new ProcessObserver<FileTransferBatchInfo, FileTransferBatchResult, FileTransferBatchErrorInfo>();
            observer.Success += info =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    var x = FileTransferBatchInfos.FirstOrDefault(x => GetFilePath(x).Equals(info.FilePath));
                    if (x is not null) FileTransferBatchInfos.Remove(x);
                    FileTransferBatchInfos.Add(info);
                });
            };

            observer.Failure += info =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    var x = FileTransferBatchInfos.FirstOrDefault(x => GetFilePath(x).Equals(info.FilePath));
                    if (x is not null) FileTransferBatchInfos.Remove(x);
                    FileTransferBatchInfos.Add(info);
                });
            };

            FileTransferBatchResult result =
                await _fileTransferBatchService.ProcessFilesAsync
                    (ExpandToFileMetadataEntry(nodes), observer);

            CreateHierarchicalModel();

            _notificationServices.Show(new Notification("重试传输完成",
                $"成功 {result.Succeed} 个, 失败 {result.Failed} 个",
                result.Failed > 0 ? NotificationType.Warning : NotificationType.Success), this);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "重试传输异常");
        }

        return;

        static IEnumerable<FileMetadataEntry> ExpandToFileMetadataEntry(IReadOnlyList<MetadataTreeNodeBase> nodes)
        {
            return nodes.OfType<ITransferredFileNode>().Concat(
                    nodes
                        .Where(x => x.HasChildren)
                        .Select(x => x.FindChildren<ITransferredFileNode>())
                        .SelectMany(x => x))
                .Select(ITransferredFileNodeToFileMetadataEntry).WhereNotNull();

            static FileMetadataEntry? ITransferredFileNodeToFileMetadataEntry(ITransferredFileNode transferredFileNode)
            {
                return transferredFileNode is MetadataTreeNodeBase { Parent: IFileMetadata fileMetadata }
                    ? new FileMetadataEntry(transferredFileNode.FullPath, fileMetadata.Metadata)
                    : null;
            }
        }

        static string GetFilePath(IFileTransferBatchInfo info) => info switch
        {
            FileTransferBatchInfo batch => batch.FilePath,
            FileTransferBatchErrorInfo error => error.FilePath,
            _ => throw new ArgumentOutOfRangeException(nameof(info), info, null)
        };
    }

    private static List<MetadataTreeNodeBase> CollectSelectedNodes(IReadOnlyList<MetadataTreeNodeBase> nodes)
    {
        var result = new List<MetadataTreeNodeBase>(nodes.Count);
        foreach (MetadataTreeNodeBase node in nodes)
        {
            bool canAdd = true;
            MetadataTreeNodeBase? removedNode = null;
            foreach (MetadataTreeNodeBase other in result)
            {
                var relationship = node.GetRelationship(other);
                if (relationship is NodeRelationship.None) continue;
                if (relationship is NodeRelationship.Self or NodeRelationship.Descendant)
                {
                    canAdd = false;
                    break;
                }

                if (relationship is NodeRelationship.Ancestor)
                {
                    removedNode = other;
                    break;
                }
            }

            if (!canAdd) continue;
            if (removedNode is not null) result.Remove(removedNode);
            result.Add(node);
        }

        return result;
    }

    private static List<MetadataTreeNodeBase> CollectSelectedNodesFailedOnly(IReadOnlyList<MetadataTreeNodeBase> nodes)
    {
        var result = new List<MetadataTreeNodeBase>(nodes.Count);
        foreach (MetadataTreeNodeBase node in nodes)
        {
            if (node is TransferredFileNode)
                continue;

            if (node.HasChildren && !node.HasChild<FailedTransferFileNode>())
                continue;

            bool canAdd = true;
            MetadataTreeNodeBase? removedNode = null;
            foreach (MetadataTreeNodeBase other in result)
            {
                var relationship = node.GetRelationship(other);
                if (relationship is NodeRelationship.None) continue;
                if (relationship is NodeRelationship.Self or NodeRelationship.Descendant)
                {
                    canAdd = false;
                    break;
                }

                if (relationship is NodeRelationship.Ancestor)
                {
                    removedNode = other;
                    break;
                }
            }

            if (!canAdd) continue;
            if (removedNode is not null) result.Remove(removedNode);
            result.Add(node);
        }

        return result;
    }
}