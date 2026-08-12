using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.Library.Services.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Movie;
using AutoOrganize.Models.Args;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.HomeViewModels.EditorViewModels.Search;

[ViewModelRegistration]
public sealed partial class MovieSearchViewModel : ViewModelBase,
    IResultNavigationViewModel<MovieSearchArgs, MovieSearchResult?>
{
    private readonly INavigationService _navigationService;
    private readonly IMetadataService _metadataService;
    private readonly ILogger<MovieSearchViewModel> _logger;

    [ObservableProperty]
    public partial MovieSearchRequest? MovieSearchRequest { get; set; }

    [ObservableProperty]
    public partial MovieSearchResult? SelectedResult { get; set; }

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    public AvaloniaList<MovieSearchResult> Results { get; } = [];

    public CancellationToken CancellationToken { get; set; }

    public MovieSearchViewModel(
        INavigationService navigationService,
        IMetadataService metadataService,
        ILogger<MovieSearchViewModel> logger)
    {
        _navigationService = navigationService;
        _metadataService = metadataService;
        _logger = logger;
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task SearchAsync(CancellationToken token)
    {
        try
        {
            if (MovieSearchRequest is null)
                return;

            _logger.LogDebug("搜索电影: {Name} ({Year})", MovieSearchRequest.Name, MovieSearchRequest.Year);

            CancellationToken cancellationToken =
                CancellationTokenSource.CreateLinkedTokenSource(token, CancellationToken).Token;
            IEnumerable<MovieSearchResult>? result =
                await _metadataService.SearchMovieAsync(new MovieSearchRequest
                {
                    Name = string.IsNullOrWhiteSpace(SearchText) ? MovieSearchRequest?.Name : SearchText,
                    Year = string.IsNullOrWhiteSpace(SearchText) ? MovieSearchRequest?.Year : null,
                    Language = "zh-cn",
                    ProviderIds = string.IsNullOrWhiteSpace(SearchText) ? MovieSearchRequest?.ProviderIds : null
                }, cancellationToken);

            Results.Clear();
            if (result is not null)
                Results.AddRange(result);

            _logger.LogDebug("搜索电影完成: 找到 {Count} 个结果", Results.Count);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "搜索电影失败: {Name} ({Year})", MovieSearchRequest?.Name, MovieSearchRequest?.Year);
        }
    }

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        _navigationService.Pop(this, SelectedResult!);
    }

    private bool CanConfirm() => SelectedResult is not null;

    partial void OnSelectedResultChanged(MovieSearchResult? value) => ConfirmCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.Pop(this);
    }

    public void OnNavigatedTo(MovieSearchArgs args)
    {
        MovieSearchRequest = args.Request;
        SearchCommand.Execute(null);
    }
}