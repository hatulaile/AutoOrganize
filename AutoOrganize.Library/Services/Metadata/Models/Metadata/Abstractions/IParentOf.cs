namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

public interface IParentOf
{
    void AddChild(object child);
    void RemoveChild(object child);
}

public interface IParentOf<TSelf, TChild> : IParentOf
    where TSelf : IParentOf<TSelf, TChild>
    where TChild : IChildOf<TChild, TSelf>
{
    IReadOnlyList<TChild> Children { get; }

    void AddChild(TChild child);
    void RemoveChild(TChild child);

    void IParentOf.AddChild(object child) => AddChild((TChild)child);
    void IParentOf.RemoveChild(object child) => RemoveChild((TChild)child);
}