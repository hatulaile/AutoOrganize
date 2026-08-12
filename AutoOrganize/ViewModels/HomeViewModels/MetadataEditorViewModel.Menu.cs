using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Models;
using AutoOrganize.Models.Args.EditorArgs;
using AutoOrganize.Models.MenuItemViewModelContext;
using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.MetadataNodes.FileSystem;
using AutoOrganize.Models.MetadataNodes.Metadata;
using AutoOrganize.ViewModels.Abstractions;
using AutoOrganize.ViewModels.HomeViewModels.EditorViewModels;
using AutoOrganize.ViewModels.HomeViewModels.EditorViewModels.Failed;
using AutoOrganize.ViewModels.HomeViewModels.EditorViewModels.File;
using AutoOrganize.ViewModels.HomeViewModels.EditorViewModels.Metadata;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoOrganize.ViewModels.HomeViewModels;

public partial class MetadataEditorViewModel
{
    public IReadOnlyList<IMenuItem<MetadataEditorMenuItemContext>> MenuItemViewModels => field ??= CreateMenuItems();

    private IReadOnlyList<IMenuItem<MetadataEditorMenuItemContext>> CreateMenuItems() =>
    [
        new MenuItem<MetadataEditorMenuItemContext>("重新识别选中剧", ReIdentifySeriesCommand,
            static context => context.SelectedItems.Any(x => x is SeriesMetadataTreeNode)),
        new MenuItem<MetadataEditorMenuItemContext>("重新识别选中季", ReIdentifySeasonCommand,
            static context => context.SelectedItems.Any(x => x is SeasonMetadataTreeNode)),
        new MenuItem<MetadataEditorMenuItemContext>("重新识别选中集", ReIdentifyEpisodeCommand,
            static context => context.SelectedItems.Any(x => x is EpisodeMetadataTreeNode)),
        new MenuItem<MetadataEditorMenuItemContext>("重新识别选中为电影", ReIdentifyMovieCommand,
            static context => context.SelectedItems.Any(x => x is MovieMetadataTreeNode)),
        new MenuItem<MetadataEditorMenuItemContext>("重新识别选中文件为电影", ReIdentifyFileAsMovieCommand,
            static context => context.SelectedItems.Any(x => x is SourceFileNode)),
        new MenuItem<MetadataEditorMenuItemContext>("重新识别选中文件为剧集", ReIdentifyFileAsTvCommand,
            static context => context.SelectedItems.Any(x => x is SourceFileNode)),
        new MenuItem<MetadataEditorMenuItemContext>("将失败文件识别为剧集", ReIdentifyFailedFileAsTvCommand,
            static context => context.SelectedItems.Any(x => x is IFailedNode)),
        new MenuItem<MetadataEditorMenuItemContext>("将失败文件识别为电影", ReIdentifyFailedFileAsMovieCommand,
            static context => context.SelectedItems.Any(x => x is IFailedNode)),
        new MenuItem<MetadataEditorMenuItemContext>("取消识别项目", UnIdentifyMetadataCommand,
            static context => context.SelectedItems.Any(x => x is not IFailedFile)),
        new MenuItem<MetadataEditorMenuItemContext>("打开源文件位置", OpenSourceFileLocationCommand,
            static context => context.SelectedItems is [IFullPath]),
    ];

    private void ApplyFileResult(SourceFileNode file, MetadataBase? metadata, Exception? error)
    {
        if (metadata is null)
        {
            _logger.LogWarning(error, "文件识别结果缺少元数据, 已跳过: {FilePath}", file.FullPath);
            return;
        }

        _logger.LogDebug("应用识别结果: {FilePath}", file.FullPath);
        file.RemoveFromParent();
        _metadataTreeRoot.AddFile(metadata, file);
    }

    private void ApplyFailedFileResult(MetadataEditorMenuItemContext context,
        FailedFileNode file, MetadataBase? metadata, Exception? error)
    {
        if (metadata is null && error is null)
            return;

        file.RemoveFromParent();
        if (metadata is not null)
        {
            _logger.LogDebug("应用失败文件识别结果: {FilePath}", file.FullPath);
            context.MetadataTreeRoot.AddFile(metadata, new SourceFileNode(file.FullPath));
        }
        else if (error is not null)
        {
            _logger.LogWarning(error, "失败文件重新识别失败, 保留在失败列表: {FilePath}", file.FullPath);
            context.FailedSourceFileRoot.AddOrGetFailedMetadata(file.FullPath, error);
        }
    }

    [RelayCommand]
    private async Task ReIdentifySeriesAsync(MetadataEditorMenuItemContext? context)
    {
        if (context is null)
            return;

        IReadOnlyList<SeriesMetadataTreeNode> nodes = context.SelectedItems.OfType<SeriesMetadataTreeNode>().ToList();
        if (nodes.Count == 0)
            return;

        try
        {
            IEnumerable<TvIdentifyResult>? results = await _windowService.ShowDialog
            <MetadataEditorWindowViewModel<SeriesMetadataEditorViewModel, SeriesMetadataEditorArgs,
                    IEnumerable<TvIdentifyResult>?>,
                MetadataEditorArgs<SeriesMetadataEditorArgs>, IEnumerable<TvIdentifyResult>?>(
                new MetadataEditorArgs<SeriesMetadataEditorArgs>(new SeriesMetadataEditorArgs(nodes)),
                this);

            if (results is null)
                return;

            foreach (TvIdentifyResult result in results)
                ApplyFileResult(result.SourceFile, result.Metadata, result.Error);

            foreach (SeriesMetadataTreeNode node in nodes)
                node.Parent?.RemoveEmptyParentInChild(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "重新识别剧失败");
        }
    }

    [RelayCommand]
    private async Task ReIdentifySeasonAsync(MetadataEditorMenuItemContext? context)
    {
        if (context is null)
            return;

        IReadOnlyList<SeasonMetadataTreeNode> nodes = context.SelectedItems.OfType<SeasonMetadataTreeNode>().ToList();
        if (nodes.Count == 0)
            return;

        try
        {
            IEnumerable<TvIdentifyResult>? results = await _windowService.ShowDialog
            <MetadataEditorWindowViewModel<SeasonMetadataEditorViewModel, SeasonMetadataEditorArgs,
                    IEnumerable<TvIdentifyResult>?>,
                MetadataEditorArgs<SeasonMetadataEditorArgs>, IEnumerable<TvIdentifyResult>?>(
                new MetadataEditorArgs<SeasonMetadataEditorArgs>(new SeasonMetadataEditorArgs(nodes)),
                this);

            if (results is null)
                return;

            foreach (TvIdentifyResult result in results)
                ApplyFileResult(result.SourceFile, result.Metadata, result.Error);

            foreach (SeasonMetadataTreeNode node in nodes)
                node.Parent?.RemoveEmptyParentInChild(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "重新识别季失败");
        }
    }

    [RelayCommand]
    private async Task ReIdentifyEpisodeAsync(MetadataEditorMenuItemContext? context)
    {
        if (context is null)
            return;

        IReadOnlyList<EpisodeMetadataTreeNode> nodes = context.SelectedItems.OfType<EpisodeMetadataTreeNode>().ToList();
        if (nodes.Count == 0)
            return;

        try
        {
            IEnumerable<TvIdentifyResult>? results = await _windowService.ShowDialog
            <MetadataEditorWindowViewModel<EpisodeMetadataEditorViewModel, EpisodeMetadataEditorArgs,
                    IEnumerable<TvIdentifyResult>?>,
                MetadataEditorArgs<EpisodeMetadataEditorArgs>, IEnumerable<TvIdentifyResult>?>(
                new MetadataEditorArgs<EpisodeMetadataEditorArgs>(new EpisodeMetadataEditorArgs(nodes)),
                this);

            if (results is null)
                return;

            foreach (TvIdentifyResult result in results)
                ApplyFileResult(result.SourceFile, result.Metadata, result.Error);

            foreach (EpisodeMetadataTreeNode node in nodes)
                node.Parent?.Parent?.RemoveEmptyParentInChild(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "重新识别剧集失败");
        }
    }

    [RelayCommand]
    private async Task ReIdentifyMovieAsync(MetadataEditorMenuItemContext? context)
    {
        if (context is null)
            return;

        IReadOnlyList<MovieMetadataTreeNode> nodes = context.SelectedItems.OfType<MovieMetadataTreeNode>().ToList();
        if (nodes.Count == 0)
            return;

        try
        {
            IEnumerable<MovieIdentifyResult>? results = await _windowService.ShowDialog
            <MetadataEditorWindowViewModel<MovieMetadataEditorViewModel, MovieMetadataEditorArgs,
                    IEnumerable<MovieIdentifyResult>?>,
                MetadataEditorArgs<MovieMetadataEditorArgs>, IEnumerable<MovieIdentifyResult>?>(
                new MetadataEditorArgs<MovieMetadataEditorArgs>(new MovieMetadataEditorArgs(nodes)),
                this);

            if (results is null)
                return;

            foreach (MovieIdentifyResult result in results)
                ApplyFileResult(result.SourceFile, result.Metadata, result.Error);

            foreach (MovieMetadataTreeNode node in nodes)
                node.Parent?.RemoveEmptyParentInChild(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "重新识别电影失败");
        }
    }

    [RelayCommand]
    private async Task ReIdentifyFileAsMovieAsync(MetadataEditorMenuItemContext? context)
    {
        if (context is null)
            return;

        IReadOnlyList<SourceFileNode> fileNodes = [.. context.SelectedItems.OfType<SourceFileNode>()];
        if (fileNodes.Count == 0)
            return;
        try
        {
            IEnumerable<MovieIdentifyResult>? results = await _windowService.ShowDialog
            <MetadataEditorWindowViewModel<MovieFileEditorViewModel, MovieFileEditorArgs,
                    IEnumerable<MovieIdentifyResult>?>,
                MetadataEditorArgs<MovieFileEditorArgs>, IEnumerable<MovieIdentifyResult>?>(
                new MetadataEditorArgs<MovieFileEditorArgs>(new MovieFileEditorArgs(fileNodes)),
                this);

            if (results is null)
                return;

            foreach (var result in results)
            {
                if (result.Metadata is null)
                    continue;
                ApplyFileResult(result.SourceFile, result.Metadata, result.Error);
                CleanupParent(result.SourceFile);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "重新识别文件为电影失败");
        }
    }

    [RelayCommand]
    private async Task ReIdentifyFileAsTvAsync(MetadataEditorMenuItemContext? context)
    {
        if (context is null)
            return;

        IReadOnlyList<SourceFileNode> fileNodes = [.. context.SelectedItems.OfType<SourceFileNode>()];
        if (fileNodes.Count == 0)
            return;

        try
        {
            IEnumerable<TvIdentifyResult>? results = await _windowService.ShowDialog
            <MetadataEditorWindowViewModel<TvFileEditorViewModel, TvFileEditorArgs,
                    IEnumerable<TvIdentifyResult>?>,
                MetadataEditorArgs<TvFileEditorArgs>, IEnumerable<TvIdentifyResult>?>(
                new MetadataEditorArgs<TvFileEditorArgs>(new TvFileEditorArgs(fileNodes)),
                this);

            if (results is null)
                return;

            foreach (TvIdentifyResult result in results)
            {
                if (result.Metadata is null)
                    continue;
                ApplyFileResult(result.SourceFile, result.Metadata, result.Error);
                CleanupParent(result.SourceFile);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "重新识别文件为剧集失败");
        }
    }

    private static void CleanupParent(MetadataTreeNodeBase? parent)
    {
        switch (parent)
        {
            case MovieMetadataTreeNode movieNode:
                movieNode.Parent?.RemoveEmptyParentInChild(true);
                break;
            case EpisodeMetadataTreeNode episodeNode:
                episodeNode.Parent?.Parent?.RemoveEmptyParentInChild(true);
                break;
        }
    }

    [RelayCommand]
    private async Task ReIdentifyFailedFileAsMovieAsync(MetadataEditorMenuItemContext? context)
    {
        if (context is null)
            return;

        IReadOnlyList<IFailedNode> nodes = [.. context.SelectedItems.OfType<IFailedNode>()];
        if (nodes.Count == 0)
            return;

        try
        {
            IEnumerable<FailedMovieIdentifyResult>? movie = await _windowService.ShowDialog
            <MetadataEditorWindowViewModel<FailedMovieEditorViewModel, FailedMovieEditorArgs,
                    IEnumerable<FailedMovieIdentifyResult>?>,
                MetadataEditorArgs<FailedMovieEditorArgs>, IEnumerable<FailedMovieIdentifyResult>?>(
                new MetadataEditorArgs<FailedMovieEditorArgs>(new FailedMovieEditorArgs(nodes)),
                this);

            if (movie is null)
                return;

            foreach (var result in movie)
            {
                if (result.Metadata is null)
                    continue;
                ApplyFailedFileResult(context, result.FailedFile, result.Metadata, result.Error);
            }

            context.FailedSourceFileRoot.RemoveEmptyParentInChild(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "重新识别失败文件为电影失败");
        }
    }

    [RelayCommand]
    private async Task ReIdentifyFailedFileAsTvAsync(MetadataEditorMenuItemContext? context)
    {
        if (context is null)
            return;

        IReadOnlyList<IFailedNode> nodes = context.SelectedItems.OfType<IFailedNode>().ToList();
        if (nodes.Count == 0)
            return;

        try
        {
            IEnumerable<FailedTvIdentifyResult>? episodes = await _windowService.ShowDialog
            <MetadataEditorWindowViewModel<FailedTvEditorViewModel, FailedTvEditorArgs,
                    IEnumerable<FailedTvIdentifyResult>?>,
                MetadataEditorArgs<FailedTvEditorArgs>, IEnumerable<FailedTvIdentifyResult>?>(
                new MetadataEditorArgs<FailedTvEditorArgs>(new FailedTvEditorArgs(nodes)),
                this);

            if (episodes is null)
                return;

            foreach (var result in episodes)
            {
                if (result.Metadata is null)
                    continue;
                ApplyFailedFileResult(context, result.FailedFile, result.Metadata, result.Error);
            }

            context.FailedSourceFileRoot.RemoveEmptyParentInChild(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "重新识别失败文件为剧集失败");
        }
    }

    [RelayCommand]
    private void UnIdentifyMetadata(MetadataEditorMenuItemContext? context)
    {
        if (context is null)
            return;

        foreach (var node in context.SelectedItems.ToArray())
        {
            if (node.FindRootParent() is not MetadataTreeRoot)
                continue;

            node.RemoveFromParent();

            foreach (SourceFileNode fileNode in node.FindChildren<SourceFileNode>())
                _failedSourceFileRoot.AddFailedSourceFile(fileNode, new Exception("手动取消识别"));
            _metadataTreeRoot.RemoveEmptyParentInChild(false);
        }
    }

    [RelayCommand]
    private async Task OpenSourceFileLocationAsync(MetadataEditorMenuItemContext? context)
    {
        try
        {
            IFullPath? node = (IFullPath?)context?.SelectedItems[0];
            if (node is null)
                return;

            string? directory = Path.GetDirectoryName(node.FullPath);
            if (directory is null)
                return;

            await _launcherServices.LaunchDirectoryInfoAsync(new DirectoryInfo(directory), this);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "打开源文件位置失败");
        }
    }
}