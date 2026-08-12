using Avalonia.Controls;
using Avalonia.Input;
using AutoOrganize.ViewModels.HomeViewModels.EditorViewModels.Search;

namespace AutoOrganize.Views.HomeViews.EditorViews.Search;

public partial class SeriesSearchView : UserControl
{
    public SeriesSearchView()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => SearchTextBox.Focus();
    }

    private void OnResultsListDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is SeriesSearchViewModel viewModel
            && e.Source is Control source
            && !ReferenceEquals(source.DataContext, DataContext)
            && viewModel.SelectedResult is not null)
        {
            viewModel.ConfirmCommand.Execute(null);
        }
    }
}
