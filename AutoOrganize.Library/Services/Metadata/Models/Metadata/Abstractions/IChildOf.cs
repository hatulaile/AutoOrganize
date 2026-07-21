namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

public interface IChildOf
{
    object? Parent { get; set; }
}

public interface IChildOf<TSelf, TParent> : IChildOf
    where TSelf : IChildOf<TSelf, TParent>
    where TParent : IParentOf<TParent, TSelf>
{
    new TParent? Parent { get; set; }

    object? IChildOf.Parent
    {
        get => Parent;
        set => Parent = (TParent?)value;
    }
}