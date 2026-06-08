using System;
using AutoOrganize.Services.NavigationServices;

namespace AutoOrganize.ViewModels.Abstractions;

public static class ViewModelExtensions
{
    extension(IViewModel viewModel)
    {
        public RoutingState FindNearestRouter()
        {
            if (viewModel is ISubNavigateViewModel subNavigateViewModel)
                return subNavigateViewModel.RoutingState;

            var current = viewModel.OwnerViewModel;
            while (current is not null)
            {
                if (current is ISubNavigateViewModel sub)
                    return sub.RoutingState;
                current = current.OwnerViewModel;
            }

            throw new InvalidOperationException("Cannot find nearest router");
        }

        public RoutingState FindAncestorRouter()
        {
            var current = viewModel.OwnerViewModel;
            while (current is not null)
            {
                if (current is ISubNavigateViewModel sub)
                    return sub.RoutingState;
                current = current.OwnerViewModel;
            }

            throw new InvalidOperationException("Cannot find nearest router");
        }
    }

    extension(INavigationService navigationService)
    {
        public void NavigateTo<TViewModel>(IViewModel context, TViewModel? defaultViewModel = default)
            where TViewModel : INavigationViewModel
        {
            navigationService.NavigateTo(context.FindAncestorRouter(), defaultViewModel);
        }

        public void NavigateTo(IViewModel context, Type viewModelType)
        {
            navigationService.NavigateTo(context.FindAncestorRouter(), viewModelType);
        }

        public void NavigateTo<TViewModel, TArgs>(IViewModel context, TArgs args, TViewModel? defaultViewModel = default)
            where TViewModel : INavigationViewModel<TArgs>
        {
            navigationService.NavigateTo(context.FindAncestorRouter(), args, defaultViewModel);
        }

        public void NavigateTo<TViewModel, TArgs>(IViewModel context, TArgs args, Type viewModelType)
        {
            navigationService.NavigateTo<TViewModel, TArgs>(context.FindAncestorRouter(), args, viewModelType);
        }

        public void Clear(IViewModel context)
        {
            navigationService.Clear(context.FindAncestorRouter());
        }
    }
}