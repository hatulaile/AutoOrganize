using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.Library.Services.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Tv;
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
public sealed partial class SeriesSearchViewModel : ViewModelBase,
    IResultNavigationViewModel<SeriesSearchArgs, SeriesSearchResult?>
{
    private readonly INavigationService _navigationService;
    private readonly IMetadataService _metadataService;
    private readonly ILogger<SeriesSearchViewModel> _logger;

    [ObservableProperty]
    public partial SeriesSearchRequest? SeriesSearchRequest { get; set; }

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    public partial SeriesSearchResult? SelectedResult { get; set; }

    public AvaloniaList<SeriesSearchResult> Results { get; } = [];

    public CancellationToken CancellationToken { get; set; }

    public SeriesSearchViewModel(
        INavigationService navigationService,
        IMetadataService metadataService,
        ILogger<SeriesSearchViewModel> logger)
    {
        _navigationService = navigationService;
        _metadataService = metadataService;
        _logger = logger;
    }

    [RelayCommand(IncludeCancelCommand = true, CanExecute = nameof(CanSearch))]
    private async Task SearchAsync(CancellationToken token)
    {
        try
        {
            if (SeriesSearchRequest is null)
                return;

            _logger.LogDebug("搜索电视剧: {Name} ({Year})", SeriesSearchRequest.Name, SeriesSearchRequest.FirstAirDateYear);

            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, CancellationToken).Token;
            IEnumerable<SeriesSearchResult>? result =
                await _metadataService.SearchSeriesAsync(new SeriesSearchRequest
                {
                    Name = string.IsNullOrWhiteSpace(SearchText) ? SeriesSearchRequest.Name : SearchText,
                    FirstAirDateYear = string.IsNullOrWhiteSpace(SearchText)
                        ? SeriesSearchRequest.FirstAirDateYear
                        : null,
                    Language = "zh-cn",
                    ProviderIds = string.IsNullOrWhiteSpace(SearchText) ? SeriesSearchRequest.ProviderIds : null
                }, linkedToken);

            linkedToken.ThrowIfCancellationRequested();
            Results.Clear();
            if (result is not null)
                Results.AddRange(result);

            _logger.LogDebug("搜索电视剧完成: 找到 {Count} 个结果", Results.Count);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "搜索电视剧失败: {Name} ({Year})",
                SeriesSearchRequest?.Name, SeriesSearchRequest?.FirstAirDateYear);
        }
    }

    private bool CanSearch() => !string.IsNullOrWhiteSpace(SearchText);

    partial void OnSearchTextChanged(string? value) => SearchCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        _navigationService.Pop(this, SelectedResult!);
    }

    private bool CanConfirm() => SelectedResult is not null;

    partial void OnSelectedResultChanged(SeriesSearchResult? value) => ConfirmCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.Pop(this);
    }

    public void OnNavigatedTo(SeriesSearchArgs args)
    {
        SeriesSearchRequest = args.Request;
        SearchText = args.Request.Name;
        SearchCommand.Execute(null);
    }
}