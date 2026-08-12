using System.ComponentModel;

namespace AutoOrganize.ViewModels.Abstractions;

public interface IViewModel : INotifyPropertyChanged
{
    IParentViewModel? OwnerViewModel { get; set; }
}