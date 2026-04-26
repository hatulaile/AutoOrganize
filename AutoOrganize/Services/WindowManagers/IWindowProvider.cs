using System.Collections.Generic;
using AutoOrganize.ViewModels;
using Avalonia.Controls;

namespace AutoOrganize.Services.WindowManagers;

public interface IWindowProvider
{
    IReadOnlyList<Window> Windows { get; }

    Window? GetWindowByViewModel(object viewModel);

    Window GetRequiredWindowByViewModel(object viewModel);

    Window? GetWindowByViewModel(ViewModelBase viewModel);

    Window GetRequiredWindowByViewModel(ViewModelBase viewModel);
}