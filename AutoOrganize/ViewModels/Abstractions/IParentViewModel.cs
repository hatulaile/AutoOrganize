using System.Collections.Generic;

namespace AutoOrganize.ViewModels.Abstractions;

public interface IParentViewModel : IViewModel
{
    IReadOnlyList<IViewModel> Children { get; }

    void AddChild(IViewModel child);

    void RemoveChild(IViewModel child);
}