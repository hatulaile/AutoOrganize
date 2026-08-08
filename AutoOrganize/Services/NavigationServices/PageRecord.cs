using AutoOrganize.ViewModels.Abstractions;

namespace AutoOrganize.Services.NavigationServices;

public sealed record PageRecord(IViewModel ViewModel, INavigationCompletion? Result);
