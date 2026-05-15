using System;
using System.Collections.Generic;
using System.Linq;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Models;
using AutoOrganize.Models.MetadataViewModels;
using AutoOrganize.Models.MetadataViewModels.FileSystem;
using AutoOrganize.Models.MetadataViewModels.Metadata;
using AutoOrganize.Models.Options;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.ViewModels.HomeViewModels.MetadataViewModels;
using Avalonia.Collections;
using Avalonia.Controls.DataGridHierarchical;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.HomeViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public sealed partial class MetadataEditViewModel : ViewModelBase, INavigationViewModel<MetadataEditOption>
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<MetadataEditViewModel> _logger;

    private MetadataRoot _metadataRoot = new();

    private FailedMetadataRoot _failedMetadataSystemRoot = new();

    [ObservableProperty]
    public partial FileMetadataBase? SelectedMetadata { get; set; }

    public IReadOnlyList<HierarchicalNode<FileMetadataBase>>? Rows => Model?.Flattened;

    public AvaloniaList<FileMetadataBase> Source { get; } = [];

    public HierarchicalModel<FileMetadataBase>? Model { get; private set; }

    public RoutingState RoutingState { get; }

    [RelayCommand(CanExecute = nameof(CanNext))]
    public void Next()
    {
        _logger.LogInformation("进入文件传输处理页面.");
        _navigationService.NavigateTo<FileTransferProcessedViewModel, FileTransferProcessedOption>(HostScreens.Home,
            new FileTransferProcessedOption(GetAllFileMetadataEntries(_metadataRoot)));
    }

    [RelayCommand]
    public void Back()
    {
        _logger.LogDebug("返回文件选择页");
        _navigationService.NavigateTo<SelectFilesViewModel>(HostScreens.Home);
    }

    public bool CanNext()
    {
        return Source.Any(x => x is not FailedMetadataRoot);
    }

    partial void OnSelectedMetadataChanged(FileMetadataBase? value)
    {
        if (value is null)
        {
            _logger.LogDebug("取消选择元数据项");
            _navigationService.Clear(HostScreens.MetadataEdit);
            return;
        }

        _logger.LogDebug("选中元数据项: {Type} - {Name}", value.GetType().Name, value.Title);
        switch (value)
        {
            case IFileMetadata<MetadataBase> metadata:
                _navigationService.NavigateTo<MetadataViewModel, MetadataBase>
                    (HostScreens.MetadataEdit, metadata.Metadata);
                break;
            case FileModel fileMetadata:
                _navigationService.NavigateTo<FileMetadataViewModel, FileModel>
                    (HostScreens.MetadataEdit, fileMetadata);
                break;
            case FailedDirectoryModel failedDirectoryMetadata:
                _navigationService.NavigateTo<FailedDirectoryMetadataViewModel, FailedDirectoryModel>(
                    HostScreens.MetadataEdit, failedDirectoryMetadata);
                break;
            case FailedMetadataRoot failedFileMetadataRoot:
                _navigationService.NavigateTo<FailedFileMetadataRootViewModel, FailedMetadataRoot>(
                    HostScreens.MetadataEdit, failedFileMetadataRoot);
                break;
            case FailedFile failedMetadata:
                _navigationService.NavigateTo<FailedFileMetadataViewModel, FailedFile>
                    (HostScreens.MetadataEdit, failedMetadata);
                break;
            default:
                _navigationService.Clear(HostScreens.MetadataEdit);
                break;
        }
    }

    public void OnParametersChanged(MetadataEditOption option)
    {
        _logger.LogDebug("参数变更，IsClear: {IsClear}, 处理结果数量: {Count}",
            option.IsClear, option.FileProcessResultInfos?.Count() ?? 0);
        CreateSource(option);
    }

    private void CreateSource(MetadataEditOption options)
    {
        if (options.IsClear)
        {
            _logger.LogDebug("清空现有源数据");
            _metadataRoot = new MetadataRoot();
            _failedMetadataSystemRoot = new FailedMetadataRoot();
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
                    _metadataRoot.AddFile(result.Metadata, result.FilePath);
                    successCount++;
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "添加成功元数据到树失败: {FilePath}", result.FilePath);
                    _failedMetadataSystemRoot.AddOrGetFailedMetadata(result.FilePath, e);
                    failedCount++;
                }
            }
            else
            {
                _failedMetadataSystemRoot.AddOrGetFailedMetadata(result.FilePath, result.Error,
                    options.FileProcessOptions.Value);
                failedCount++;
            }
        }

        _logger.LogDebug("构建源数据完成: 成功 {Success}, 失败 {Failed}", successCount, failedCount);

        if (options.IsClear)
        {
            if (_failedMetadataSystemRoot.Children.Count > 0)
            {
                Source.Add(_failedMetadataSystemRoot);
            }

            Source.AddRange(_metadataRoot.Children);
        }

        if (Model is null)
        {
            Model = new HierarchicalModel<FileMetadataBase>(new HierarchicalOptions<FileMetadataBase>
            {
                ChildrenSelector = x => x.Children,
                IsLeafSelector = x => !x.HasChildren,
                VirtualizeChildren = true,
            });

            Model.SetRoots(Source);
            _logger.LogDebug("分层模型已创建");
        }
    }

    private static IEnumerable<FileMetadataEntry> GetAllFileMetadataEntries(FileMetadataBase fileMetadata)
    {
        if (fileMetadata is FileEpisodeMetadata episodeMetadata)
        {
            foreach (FileMetadataBase episodeMetadataChild in episodeMetadata.Children)
            {
                if (episodeMetadataChild is not FileModel metadataChild)
                    continue;
                yield return new FileMetadataEntry(metadataChild.FullPath, episodeMetadata.Metadata);
            }
        }

        if (!fileMetadata.HasChildren) yield break;
        foreach (FileMetadataBase fileMetadataChildren in fileMetadata.Children)
        {
            foreach (var allFileMetadataEntry in GetAllFileMetadataEntries(fileMetadataChildren))
            {
                yield return allFileMetadataEntry;
            }
        }
    }

    public MetadataEditViewModel(
        INavigationService navigationViewModel,
        [FromKeyedServices(HostScreens.MetadataEdit)]
        RoutingState routingState, ILogger<MetadataEditViewModel> logger)
    {
        _navigationService = navigationViewModel;
        _logger = logger;
        RoutingState = routingState;
        RoutingState.SetOwnerViewModel(this);
    }
}