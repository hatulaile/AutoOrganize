using System.Collections.Generic;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls;

namespace AutoOrganize.Services.WindowManagers;

public interface IWindowProvider
{
    IReadOnlyList<Window> Windows { get; }

    Window? GetWindowByViewModel(object viewModel);

    Window GetRequiredWindowByViewModel(object viewModel);

    Window? GetWindowByViewModel(IViewModel viewModel);

    Window GetRequiredWindowByViewModel(IViewModel viewModel);
}