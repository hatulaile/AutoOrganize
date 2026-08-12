using System;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.Models.Args.EditorArgs;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.WindowManagers;
using AutoOrganize.ViewModels.Abstractions;
using Microsoft.Extensions.Logging;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.HomeViewModels.EditorViewModels;

public abstract class MetadataEditorWindowViewModelBase : SubNavigateViewModelBase;

[ViewModelRegistration]
public class MetadataEditorWindowViewModel<TViewModel, TRequest, TResult> :
    MetadataEditorWindowViewModelBase,
    IResultWindowViewModel<MetadataEditorArgs<TRequest>, TResult?>
    where TViewModel : IResultNavigationViewModel<TRequest, TResult>
{
    private readonly INavigationService _navigationService;
    private readonly IWindowService _windowService;
    private readonly ILogger<MetadataEditorWindowViewModel<TViewModel, TRequest, TResult>> _logger;

    public CancellationToken CancellationToken { get; set; }

    public MetadataEditorWindowViewModel(INavigationService navigationService, IWindowService windowService,
        ILogger<MetadataEditorWindowViewModel<TViewModel, TRequest, TResult>> logger)
    {
        _navigationService = navigationService;
        _windowService = windowService;
        _logger = logger;
    }

    public void OnOpenWindow(MetadataEditorArgs<TRequest> args)
    {
        _ = NavigationAndClose(args);
    }

    private async Task NavigationAndClose(MetadataEditorArgs<TRequest> args)
    {
        try
        {
            TResult? result = await _navigationService
                .RequestAsync<TViewModel, TRequest, TResult?>(RoutingState, args.Request, CancellationToken);
            _windowService.Close(this, result);
        }
        catch (OperationCanceledException)
        {
            _windowService.Close(this, default);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "元数据编辑窗口导航请求失败");
            _windowService.Close(this, default);
        }
    }
}