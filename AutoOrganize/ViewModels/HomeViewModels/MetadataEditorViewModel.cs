using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AsyncImageLoader.Loaders;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Models;
using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.MetadataNodes.FileSystem;
using AutoOrganize.Models.MetadataNodes.Metadata;
using AutoOrganize.Models.Args;
using AutoOrganize.Models.MenuItemViewModelContext;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.TopLevelServices;
using AutoOrganize.Services.WindowManagers;
using AutoOrganize.ViewModels.Abstractions;
using AutoOrganize.ViewModels.HomeViewModels.MetadataViewModels;
using Avalonia.Collections;
using Avalonia.Controls.DataGridHierarchical;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.HomeViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton, ViewModelLifetime.Singleton)]
public sealed partial class MetadataEditorViewModel : SubNavigateViewModelBase, INavigationViewModel<MetadataEditArgs>
{
    private readonly INavigationService _navigationService;
    private readonly IWindowService _windowService;
    private readonly ILauncherServices _launcherServices;
    private readonly ILogger<MetadataEditorViewModel> _logger;

    private MetadataTreeRoot _metadataTreeRoot = new();

    private FailedSourceFileRoot _failedSourceFileRoot = new();

    public AvaloniaList<MetadataTreeNodeBase> SelectItems { get; }

    [ObservableProperty]
    public partial IMenuItemContext MenuItemContext { get; set; }

    public AvaloniaList<MetadataTreeNodeBase> Source { get; }

    public HierarchicalModel<MetadataTreeNodeBase>? Model { get; private set; }

    public MetadataEditorViewModel(
        INavigationService navigationViewModel, IWindowService windowService, ILauncherServices launcherServices,
        ILogger<MetadataEditorViewModel> logger)
    {
        _navigationService = navigationViewModel;
        _windowService = windowService;
        _launcherServices = launcherServices;
        _logger = logger;

        SelectItems = [];
        SelectItems.CollectionChanged += SelectItemsOnCollectionChanged;
        MenuItemContext = CreateMenuItemContext();
        Source = [];
        Source.CollectionChanged += SourceOnCollectionChanged;
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    public void Next()
    {
        _logger.LogInformation("进入文件传输处理页面.");
        _navigationService.Replace<FileTransferProcessedViewModel, FileTransferProcessedArgs>(this,
            new FileTransferProcessedArgs(GetAllFileMetadataEntries(_metadataTreeRoot)));
    }

    [RelayCommand]
    public void Back()
    {
        _logger.LogDebug("返回文件选择页");
        _navigationService.Replace<SelectFilesViewModel>(this);

        if (AsyncImageLoader.ImageLoader.AsyncImageLoader is RamCachedWebImageLoader ram)
            ram.ClearRamCache();
    }

    public bool CanNext()
    {
        return Source.Any(x => x is not FailedSourceFileRoot);
    }

    private MetadataEditorMenuItemContext CreateMenuItemContext() =>
        new()
        {
            SelectedItems = SelectItems,
            FailedSourceFileRoot = _failedSourceFileRoot,
            MetadataTreeRoot = _metadataTreeRoot,
        };

    private void SelectItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MenuItemContext = CreateMenuItemContext();

        if (e.NewItems is not { Count: > 0 })
            return;

        var selectedItem = (IMetadataTreeNode?)e.NewItems[0];
        if (selectedItem is null)
            return;

        switch (selectedItem)
        {
            case IFileMetadata<MetadataBase> metadata:
                _navigationService.Replace<MetadataViewModel, MetadataBase>
                    (RoutingState, metadata.Metadata);
                break;
            case SourceFileNode fileMetadata:
                _navigationService.Replace<SourceFileViewModel, SourceFileNode>
                    (RoutingState, fileMetadata);
                break;
            case FailedDirectoryNode failedDirectoryMetadata:
                _navigationService.Replace<FailedDirectoryMetadataViewModel, FailedDirectoryNode>(
                    RoutingState, failedDirectoryMetadata);
                break;
            case FailedSourceFileRoot failedFileMetadataRoot:
                _navigationService.Replace<FailedFileRootViewModel, FailedSourceFileRoot>(
                    RoutingState, failedFileMetadataRoot);
                break;
            case FailedFileNode failedMetadata:
                _navigationService.Replace<FailedFileViewModel, FailedFileNode>
                    (RoutingState, failedMetadata);
                break;
            default:
                _navigationService.Clear(RoutingState);
                break;
        }
    }

    private void SourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NextCommand.NotifyCanExecuteChanged();
    }

    public void OnParametersChanged(MetadataEditArgs args)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("参数变更，IsClear: {IsClear}, 处理结果数量: {Count}",
                args.IsClear, args.FileProcessResultInfos?.Count() ?? 0);
        CreateSource(args);
    }

    private void CreateSource(MetadataEditArgs args)
    {
        if (args.IsClear)
        {
            _logger.LogDebug("清空现有源数据");
            _metadataTreeRoot.Children.CollectionChanged -= ChildrenOnCollectionChanged;
            _metadataTreeRoot = new MetadataTreeRoot();
            _metadataTreeRoot.Children.CollectionChanged += ChildrenOnCollectionChanged;
            _failedSourceFileRoot = new FailedSourceFileRoot();
            Source.Clear();
        }

        if (args.FileProcessResultInfos is null || args.FileProcessArgs is null)
        {
            _logger.LogDebug("无处理结果或处理选项，跳过构建源");
            return;
        }

        int successCount = 0, failedCount = 0;
        foreach (FileMetadataProcessingResult result in args.FileProcessResultInfos)
        {
            if (result.IsSuccess)
            {
                try
                {
                    _metadataTreeRoot.AddFile(result.Metadata, result.FilePath);
                    successCount++;
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "添加成功元数据到树失败: {FilePath}", result.FilePath);
                    _failedSourceFileRoot.AddOrGetFailedMetadata(result.FilePath, e);
                    failedCount++;
                }
            }
            else
            {
                _failedSourceFileRoot.AddOrGetFailedMetadata(result.FilePath, result.Error,
                    args.FileProcessArgs.Value);
                failedCount++;
            }
        }

        _logger.LogDebug("构建源数据完成: 成功 {Success}, 失败 {Failed}", successCount, failedCount);

        if (args.IsClear)
        {
            if (_failedSourceFileRoot.Children.Count > 0)
                Source.Insert(0, _failedSourceFileRoot);

            //这里忽略 _metadataTreeRoot 的内容，因为下面有对应的事件
        }

        if (Model is null)
        {
            Model = new HierarchicalModel<MetadataTreeNodeBase>(new HierarchicalOptions<MetadataTreeNodeBase>
            {
                ChildrenSelector = x => x.Children,
                IsLeafSelector = x => !x.HasChildren,
                VirtualizeChildren = true,
            });

            Model.SetRoots(Source);
            _logger.LogDebug("分层模型已创建");
        }
    }

    private void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            Source.RemoveAll(e.OldItems.Cast<MetadataTreeNodeBase>());

        if (e.NewItems is not null)
            Source.AddRange(e.NewItems.Cast<MetadataTreeNodeBase>());
    }

    private static IEnumerable<FileMetadataEntry> GetAllFileMetadataEntries(IMetadataTreeNode metadataTreeNode)
    {
        if (metadataTreeNode is EpisodeMetadataTreeNode episodeMetadata)
        {
            foreach (MetadataTreeNodeBase episodeMetadataChild in episodeMetadata.Children)
            {
                if (episodeMetadataChild is not SourceFileNode metadataChild)
                    continue;
                yield return new FileMetadataEntry(metadataChild.FullPath, episodeMetadata.Metadata);
            }
        }

        if (!metadataTreeNode.HasChildren) yield break;
        foreach (MetadataTreeNodeBase fileMetadataChildren in metadataTreeNode.Children)
        {
            foreach (var allFileMetadataEntry in GetAllFileMetadataEntries(fileMetadataChildren))
            {
                yield return allFileMetadataEntry;
            }
        }
    }
}