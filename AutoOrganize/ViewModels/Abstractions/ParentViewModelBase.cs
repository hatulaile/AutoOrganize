using System.Collections.Generic;
using Avalonia.Collections;

namespace AutoOrganize.ViewModels.Abstractions;

public abstract partial class ParentViewModelBase : ViewModelBase, IParentViewModel
{
    private readonly AvaloniaList<IViewModel> _children = [];

    public IReadOnlyList<IViewModel> Children => _children;

    public void AddChild(IViewModel child)
        => _children.Add(child);

    public void RemoveChild(IViewModel child)
        => _children.Remove(child);
}