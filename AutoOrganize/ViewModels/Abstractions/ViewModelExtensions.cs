using System;
using System.Threading;
using System.Threading.Tasks;
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
        public void Replace<TViewModel>(IViewModel context)
            where TViewModel : INavigationViewModel
        {
            navigationService.Replace<TViewModel>(context.FindAncestorRouter());
        }

        public void Replace(IViewModel context, Type viewModelType)
        {
            navigationService.Replace(context.FindAncestorRouter(), viewModelType);
        }

        public void Replace<TViewModel, TArgs>(IViewModel context, TArgs args)
            where TViewModel : INavigationViewModel<TArgs>
        {
            navigationService.Replace<TViewModel, TArgs>(context.FindAncestorRouter(), args);
        }

        public void Replace<TViewModel, TArgs>(IViewModel context, TArgs args, Type viewModelType)
        {
            navigationService.Replace<TViewModel, TArgs>(context.FindAncestorRouter(), args, viewModelType);
        }

        public void Push<TViewModel>(IViewModel context)
            where TViewModel : INavigationViewModel
        {
            navigationService.Push<TViewModel>(context.FindAncestorRouter());
        }

        public void Push(IViewModel context, Type viewModelType)
        {
            navigationService.Push(context.FindAncestorRouter(), viewModelType);
        }

        public void Push<TViewModel, TArgs>(IViewModel context, TArgs args)
            where TViewModel : INavigationViewModel<TArgs>
        {
            navigationService.Push<TViewModel, TArgs>(context.FindAncestorRouter(), args);
        }

        public void Push<TViewModel, TArgs>(IViewModel context, TArgs args, Type viewModelType)
        {
            navigationService.Push<TViewModel, TArgs>(context.FindAncestorRouter(), args, viewModelType);
        }

        public Task<TResult> RequestAsync<TViewModel, TResult>(IViewModel context,
            CancellationToken token = default)
            where TViewModel : IResultNavigationViewModel<TResult>
        {
            return navigationService.RequestAsync<TViewModel, TResult>(context.FindAncestorRouter(), token);
        }

        public Task<TResult> RequestAsync<TViewModel, TResult>(IViewModel context, Type viewModelType,
            CancellationToken token = default)
            where TViewModel : IResultNavigationViewModel<TResult>
        {
            return navigationService.RequestAsync<TViewModel, TResult>(context.FindAncestorRouter(), viewModelType, token);
        }

        public Task<TResult> RequestAsync<TViewModel, TArgs, TResult>(IViewModel context, TArgs args,
            CancellationToken token = default)
            where TViewModel : IResultNavigationViewModel<TArgs, TResult>
        {
            return navigationService.RequestAsync<TViewModel, TArgs, TResult>(context.FindAncestorRouter(), args, token);
        }

        public Task<TResult> RequestAsync<TViewModel, TArgs, TResult>(IViewModel context, TArgs args, Type viewModelType,
            CancellationToken token = default)
            where TViewModel : IResultNavigationViewModel<TArgs, TResult>
        {
            return navigationService.RequestAsync<TViewModel, TArgs, TResult>(context.FindAncestorRouter(), args,
                viewModelType, token);
        }

        public void Pop(IViewModel context)
        {
            navigationService.Pop(context.FindAncestorRouter());
        }

        public void Pop<TResult>(IViewModel context, TResult result)
        {
            navigationService.Pop(context.FindAncestorRouter(), result);
        }

        public void Clear(IViewModel context)
        {
            navigationService.Clear(context.FindAncestorRouter());
        }
    }
}