using Avalonia.Controls;
using Avalonia.Input;
using AutoOrganize.ViewModels.HomeViewModels.EditorViewModels.Search;

namespace AutoOrganize.Views.HomeViews.EditorViews.Search;

public partial class MovieSearchView : UserControl
{
    public MovieSearchView()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => SearchTextBox.Focus();
    }

    private void OnResultsListDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MovieSearchViewModel viewModel
            && e.Source is Control source
            && !ReferenceEquals(source.DataContext, DataContext)
            && viewModel.SelectedResult is not null)
        {
            viewModel.ConfirmCommand.Execute(null);
        }
    }
}
