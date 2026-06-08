namespace AutoOrganize.ViewModels.Abstractions;

public interface IViewModel
{
    IParentViewModel? OwnerViewModel { get; set; }
}