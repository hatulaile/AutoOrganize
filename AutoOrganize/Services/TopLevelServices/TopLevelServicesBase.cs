using System;
using System.Linq;
using AutoOrganize.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace AutoOrganize.Services.TopLevelServices;

public abstract class TopLevelServicesBase
{
    protected TopLevel DefaultTopLevel => field ??= GetDefaultTopLevel();

    protected static TopLevel GetTopLevel(Visual visual)
    {
        return TopLevel.GetTopLevel(visual) ?? throw new NotSupportedException();
    }

    protected static TopLevel GetDefaultTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return GetTopLevel(desktop.MainWindow ?? throw new NotSupportedException());
        }

        throw new NotSupportedException();
    }

    protected static TopLevel? FindTopLevel(object dataContext)
    {
        if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Window? window;
            object? context = dataContext;
            do
            {
                window = desktop.Windows.FirstOrDefault(x => ReferenceEquals(x.DataContext, context));
                if (context is ViewModelBase navigationViewModel) context = navigationViewModel.OwnerViewModel;
                else context = null;
            } while (window is null && context is not null);

            return TopLevel.GetTopLevel(window);
        }

        return null;
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
}