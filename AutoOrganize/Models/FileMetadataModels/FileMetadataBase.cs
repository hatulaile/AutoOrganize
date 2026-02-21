using System;
using System.Linq;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Models.FileMetadataModels;

public abstract class FileMetadataBase : ObservableObject
{
    public abstract string? Title { get; }

    public abstract FileMetadataType Type { get; }

    public virtual bool HasChildren => false;

    private AvaloniaList<FileMetadataBase> ChildrenInternal =>
        HasChildren ? field ??= InitializeChildren() : throw new NotSupportedException();

    public IAvaloniaReadOnlyList<FileMetadataBase> Children => ChildrenInternal;

    public virtual void AddChild(FileMetadataBase fileMetadataBase)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        ChildrenInternal.Add(fileMetadataBase);
    }

    public virtual void InsertChild(int index, FileMetadataBase fileMetadataBase)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        ChildrenInternal.Insert(index, fileMetadataBase);
    }

    public virtual int IndexOfChild(Func<FileMetadataBase, bool> conditions)
    {
        if (!HasChildren)
            throw new NotSupportedException();
        FileMetadataBase? first = ChildrenInternal.FirstOrDefault(conditions);
        if (first is null)
            return -1;
        return IndexOfChild(first);
    }

    public virtual int IndexOfChild(FileMetadataBase fileMetadataBase)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        return ChildrenInternal.IndexOf(fileMetadataBase);
    }

    public TMetadataBase? GetChildren<TMetadataBase>(Func<TMetadataBase, bool> conditions)
    {
        foreach (FileMetadataBase fileMetadataBase in Children)
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
        foreach (FileMetadataBase fileMetadataBase in Children)
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

    private AvaloniaList<FileMetadataBase> InitializeChildren()
    {
        var list = new AvaloniaList<FileMetadataBase>();
        return list;
    }
}