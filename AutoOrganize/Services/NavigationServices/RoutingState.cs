using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoOrganize.ViewModels.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Services.NavigationServices;

public class RoutingState : ObservableObject
{
    private readonly Stack<PageRecord> _pageStack = new();

    public IParentViewModel? OwnerViewModel { get; private set; }

    public IViewModel? CurrentPageViewModel => _pageStack.TryPeek(out PageRecord? record) ? record.ViewModel : null;

    public PageRecord? CurrentPageRecord => _pageStack.TryPeek(out PageRecord? record) ? record : null;

    public bool HasPage => _pageStack.Count > 1;

    public event NavigatedHandler? Navigated;

    public void SetOwnerViewModel(IParentViewModel owner)
    {
        OwnerViewModel = owner;
    }

    public void Push(PageRecord record)
    {
        IViewModel? oldViewModel = CurrentPageViewModel;
        _pageStack.Push(record);
        NotifyChanged(oldViewModel, record.ViewModel);
    }

    public void Pop()
    {
        if (!_pageStack.TryPop(out PageRecord? record))
            return;
        record.Result?.Complete();
        NotifyChanged(record.ViewModel, CurrentPageViewModel);
        DisposePage(record.ViewModel);
    }

    public void Pop<TResult>(TResult result)
    {
        if (!_pageStack.TryPop(out PageRecord? record))
            return;
        if (record.Result is INavigationResult<TResult> navigationResult)
            navigationResult.Complete(result);
        else record.Result?.Cancel();
        NotifyChanged(record.ViewModel, CurrentPageViewModel);
        DisposePage(record.ViewModel);
    }

    public void Replace(PageRecord record)
    {
        IViewModel? oldViewModel = CurrentPageViewModel;
        if (_pageStack.TryPop(out PageRecord? old))
        {
            old.Result?.Cancel();
            DisposePage(old.ViewModel);
        }

        _pageStack.Push(record);
        NotifyChanged(oldViewModel, record.ViewModel);
    }

    public void Clear()
    {
        IViewModel? oldViewModel = CurrentPageViewModel;
        while (_pageStack.TryPop(out PageRecord? record))
        {
            record.Result?.Cancel();
            DisposePage(record.ViewModel);
        }

        NotifyChanged(oldViewModel, null);
    }

    private static void DisposePage(IViewModel page)
    {
        if (page is IDisposable disposable)
        {
            disposable.Dispose();
        }
        else if (page is IAsyncDisposable asyncDisposable)
        {
            _ = DisposePageAsync(asyncDisposable);
        }
    }

    private static async Task DisposePageAsync(IAsyncDisposable disposable)
    {
        await disposable.DisposeAsync();
    }

    private void NotifyChanged(IViewModel? oldViewModel, IViewModel? newViewModel)
    {
        OnPropertyChanged(nameof(CurrentPageViewModel));
        OnPropertyChanged(nameof(HasPage));
        Navigated?.Invoke(this, new NavigatedEventArgs(oldViewModel, newViewModel, this));
    }
}