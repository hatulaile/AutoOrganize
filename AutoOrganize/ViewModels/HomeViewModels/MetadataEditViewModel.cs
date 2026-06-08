using System;
using System.Collections.Generic;
using System.Linq;
using AsyncImageLoader.Loaders;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Models;
using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.MetadataNodes.FileSystem;
using AutoOrganize.Models.MetadataNodes.Metadata;
using AutoOrganize.Models.Options;
using AutoOrganize.Services.NavigationServices;
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
public sealed partial class MetadataEditViewModel : SubNavigateViewModelBase, INavigationViewModel<MetadataEditOption>
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<MetadataEditViewModel> _logger;

    private MetadataTreeRoot _metadataTreeRoot = new();

    private FailedSourceFileRoot _failedSourceFileRootSystem = new();

    [ObservableProperty]
    public partial MetadataTreeNodeBase? SelectedMetadata { get; set; }

    public IReadOnlyList<HierarchicalNode<MetadataTreeNodeBase>>? Rows => Model?.Flattened;

    public AvaloniaList<MetadataTreeNodeBase> Source { get; } = [];

    public HierarchicalModel<MetadataTreeNodeBase>? Model { get; private set; }

    [RelayCommand(CanExecute = nameof(CanNext))]
    public void Next()
    {
        _logger.LogInformation("进入文件传输处理页面.");
        _navigationService.NavigateTo<FileTransferProcessedViewModel, FileTransferProcessedOption>(this,
            new FileTransferProcessedOption(GetAllFileMetadataEntries(_metadataTreeRoot)));
    }

    [RelayCommand]
    public void Back()
    {
        _logger.LogDebug("返回文件选择页");
        _navigationService.NavigateTo<SelectFilesViewModel>(this);

        if (AsyncImageLoader.ImageLoader.AsyncImageLoader is RamCachedWebImageLoader ram)
            ram.ClearRamCache();
    }

    public bool CanNext()
    {
        return Source.Any(x => x is not FailedSourceFileRoot);
    }

    partial void OnSelectedMetadataChanged(MetadataTreeNodeBase? value)
    {
        if (value is null)
        {
            _logger.LogDebug("取消选择元数据项");
            _navigationService.Clear(RoutingState);
            return;
        }

        _logger.LogDebug("选中元数据项: {Type} - {Name}", value.GetType().Name, value.Title);
        switch (value)
        {
            case IFileMetadata<MetadataBase> metadata:
                _navigationService.NavigateTo<MetadataViewModel, MetadataBase>
                    (RoutingState, metadata.Metadata);
                break;
            case SourceFileNode fileMetadata:
                _navigationService.NavigateTo<SourceFileViewModel, SourceFileNode>
                    (RoutingState, fileMetadata);
                break;
            case FailedDirectoryNode failedDirectoryMetadata:
                _navigationService.NavigateTo<FailedDirectoryMetadataViewModel, FailedDirectoryNode>(
                    RoutingState, failedDirectoryMetadata);
                break;
            case FailedSourceFileRoot failedFileMetadataRoot:
                _navigationService.NavigateTo<FailedFileRootViewModel, FailedSourceFileRoot>(
                    RoutingState, failedFileMetadataRoot);
                break;
            case FailedFileNode failedMetadata:
                _navigationService.NavigateTo<FailedFileViewModel, FailedFileNode>
                    (RoutingState, failedMetadata);
                break;
            default:
                _navigationService.Clear(RoutingState);
                break;
        }
    }

    public void OnParametersChanged(MetadataEditOption option)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("参数变更，IsClear: {IsClear}, 处理结果数量: {Count}",
                option.IsClear, option.FileProcessResultInfos?.Count() ?? 0);
        CreateSource(option);
    }

    private void CreateSource(MetadataEditOption options)
    {
        if (options.IsClear)
        {
            _logger.LogDebug("清空现有源数据");
            _metadataTreeRoot = new MetadataTreeRoot();
            _failedSourceFileRootSystem = new FailedSourceFileRoot();
            Source.Clear();
        }

        if (options.FileProcessResultInfos is null || options.FileProcessOptions is null)
        {
            _logger.LogDebug("无处理结果或处理选项，跳过构建源");
            return;
        }

        int successCount = 0, failedCount = 0;
        foreach (FileMetadataProcessingResult result in options.FileProcessResultInfos)
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
                    _failedSourceFileRootSystem.AddOrGetFailedMetadata(result.FilePath, e);
                    failedCount++;
                }
            }
            else
            {
                _failedSourceFileRootSystem.AddOrGetFailedMetadata(result.FilePath, result.Error,
                    options.FileProcessOptions.Value);
                failedCount++;
            }
        }

        _logger.LogDebug("构建源数据完成: 成功 {Success}, 失败 {Failed}", successCount, failedCount);

        if (options.IsClear)
        {
            if (_failedSourceFileRootSystem.Children.Count > 0)
            {
                Source.Add(_failedSourceFileRootSystem);
            }

            Source.AddRange(_metadataTreeRoot.Children);
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

    private static IEnumerable<FileMetadataEntry> GetAllFileMetadataEntries(MetadataTreeNodeBase metadataTreeNode)
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

    public MetadataEditViewModel(
        INavigationService navigationViewModel, ILogger<MetadataEditViewModel> logger)
    {
        _navigationService = navigationViewModel;
        _logger = logger;
    }
}