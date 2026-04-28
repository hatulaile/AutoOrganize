using System;
using System.Linq;
using AutoOrganize.Services.WindowManagers;
using AutoOrganize.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace AutoOrganize.Services.TopLevelServices;

public abstract class TopLevelServicesBase
{
    private readonly IWindowProvider _windowProvider;
    protected TopLevel DefaultTopLevel => field ??= GetDefaultTopLevel();

    protected TopLevel GetTopLevel(Visual visual)
    {
        return TopLevel.GetTopLevel(visual) ?? throw new NotSupportedException();
    }

    protected TopLevel GetDefaultTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return GetTopLevel(desktop.MainWindow ?? throw new NotSupportedException());
        }

        throw new NotSupportedException();
    }

    protected TopLevel? FindTopLevel(object dataContext)
    {
        return TopLevel.GetTopLevel(_windowProvider.GetWindowByViewModel(dataContext));
    }

    protected TopLevelServicesBase(IWindowProvider windowProvider)
    {
        _windowProvider = windowProvider;
    }
}

public abstract class TopLevelServicesBase<TProvider> : TopLevelServicesBase
{
    protected TProvider Default => field ??= GetProvider(DefaultTopLevel);

    protected abstract TProvider GetProvider(TopLevel topLevel);

    protected virtual TProvider GetProvider(Visual visual) =>
        GetProvider(GetTopLevel(visual));

    protected virtual TProvider? GetProvider(object dataContext)
    {
        TopLevel? topLevel = FindTopLevel(dataContext);
        return topLevel is null ? default : GetProvider(topLevel);
    }

    protected virtual TProvider GetProviderOrDefault(object? dataContext)
    {
        if (dataContext is null)
            return Default;
        TopLevel? topLevel = FindTopLevel(dataContext);
        return topLevel is null ? Default : GetProvider(topLevel);
    }

    protected TopLevelServicesBase(IWindowProvider windowProvider) : base(windowProvider)
    {
    }
}