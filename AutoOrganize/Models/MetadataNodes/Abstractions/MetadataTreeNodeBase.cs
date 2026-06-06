using System;
using System.Linq;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Models.MetadataNodes.Abstractions;

public abstract class MetadataTreeNodeBase : ObservableObject
{
    public abstract string? Title { get; }

    public abstract MetadataNodeType NodeType { get; }

    public virtual bool HasChildren => false;

    private AvaloniaList<MetadataTreeNodeBase> ChildrenInternal =>
        HasChildren ? field ??= InitializeChildren() : throw new NotSupportedException();

    public IAvaloniaReadOnlyList<MetadataTreeNodeBase> Children => ChildrenInternal;

    public virtual void AddChild(MetadataTreeNodeBase metadataTreeNodeBase)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        ChildrenInternal.Add(metadataTreeNodeBase);
    }

    public virtual void InsertChild(int index, MetadataTreeNodeBase metadataTreeNodeBase)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        ChildrenInternal.Insert(index, metadataTreeNodeBase);
    }

    public virtual int IndexOfChild(Func<MetadataTreeNodeBase, bool> conditions)
    {
        if (!HasChildren)
            throw new NotSupportedException();
        MetadataTreeNodeBase? first = ChildrenInternal.FirstOrDefault(conditions);
        if (first is null)
            return -1;
        return IndexOfChild(first);
    }

    public virtual int IndexOfChild(MetadataTreeNodeBase metadataTreeNodeBase)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        return ChildrenInternal.IndexOf(metadataTreeNodeBase);
    }

    public TMetadataBase? GetChildren<TMetadataBase>(Func<TMetadataBase, bool> conditions)
    {
        foreach (MetadataTreeNodeBase fileMetadataBase in Children)
        {
            if (fileMetadataBase is not TMetadataBase metadataBase)
                continue;

            if (conditions(metadataBase))
                return metadataBase;
        }

        return default;
    }

    public TMetadataBase? GetChildren<TMetadataBase, TSubMetadataBase>(Func<TMetadataBase, bool> conditions,
        Func<TSubMetadataBase, bool> subConditions)
    {
        foreach (MetadataTreeNodeBase fileMetadataBase in Children)
        {
            if (fileMetadataBase is TMetadataBase metadataBase)
            {
                if (conditions(metadataBase))
                    return metadataBase;
            }

            if (fileMetadataBase.HasChildren && fileMetadataBase is TSubMetadataBase subMetadataBase)
            {
                if (!subConditions(subMetadataBase))
                    continue;

                TMetadataBase? metadata = fileMetadataBase.GetChildren(conditions, subConditions);
                if (metadata is not null)
                    return metadata;
            }
        }

        return default;
    }

    private AvaloniaList<MetadataTreeNodeBase> InitializeChildren()
    {
        var list = new AvaloniaList<MetadataTreeNodeBase>();
        return list;
    }
}