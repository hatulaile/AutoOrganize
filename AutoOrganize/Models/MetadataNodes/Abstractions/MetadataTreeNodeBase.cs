using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Models.MetadataNodes.Abstractions;

public abstract class MetadataTreeNodeBase : ObservableObject, IMetadataTreeNode
{
    public abstract string? Title { get; }

    public abstract MetadataNodeType NodeType { get; }

    public virtual bool HasChildren => false;

    public MetadataTreeNodeBase? Parent { get; private set; }

    private AvaloniaList<MetadataTreeNodeBase> ChildrenInternal =>
        HasChildren ? field ??= InitializeChildren() : throw new NotSupportedException();

    public IAvaloniaReadOnlyList<MetadataTreeNodeBase> Children => ChildrenInternal;

    public virtual void AddChild(MetadataTreeNodeBase metadataTreeNodeBase)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        if (metadataTreeNodeBase.Parent is not null)
            throw new InvalidOperationException(
                $"Node already has a parent and cannot be added again: {metadataTreeNodeBase.Title}");

        ChildrenInternal.Add(metadataTreeNodeBase);
        metadataTreeNodeBase.Parent = this;
    }

    public virtual void InsertChild(int index, MetadataTreeNodeBase metadataTreeNodeBase)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        if (metadataTreeNodeBase.Parent is not null)
            throw new InvalidOperationException(
                $"Node already has a parent and cannot be inserted again: {metadataTreeNodeBase.Title}");

        ChildrenInternal.Insert(index, metadataTreeNodeBase);
        metadataTreeNodeBase.Parent = this;
    }

    public bool RemoveFromParent()
    {
        return Parent?.RemoveChild(this) ?? false;
    }

    public MetadataTreeNodeBase? FindRootParent()
    {
        MetadataTreeNodeBase? root = Parent;
        while (root?.Parent is not null)
            root = root.Parent;
        return root;
    }

    public TMetadataBase? FindParent<TMetadataBase>()
    {
        MetadataTreeNodeBase? current = Parent;
        while (current is not null)
        {
            if (current is TMetadataBase metadataBase)
                return metadataBase;
            current = current.Parent;
        }

        return default;
    }

    public TMetadataBase? FindParent<TMetadataBase>(Func<TMetadataBase, bool> conditions)
    {
        MetadataTreeNodeBase? current = Parent;
        while (current is not null)
        {
            if (current is TMetadataBase metadataBase && conditions(metadataBase))
                return metadataBase;
            current = current.Parent;
        }

        return default;
    }

    public bool HasParent<TMetadataBase>()
    {
        return FindParent<TMetadataBase>() is not null;
    }

    public bool HasParent<TMetadataBase>(Func<TMetadataBase, bool> conditions)
    {
        return FindParent(conditions) is not null;
    }

    public bool HasParent(IMetadataTreeNode node)
    {
        for (MetadataTreeNodeBase? current = Parent; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, node))
                return true;
        }

        return false;
    }

    public bool HasChild<TNode>()
    {
        if (!HasChildren) throw new NotSupportedException();
        foreach (MetadataTreeNodeBase child in Children)
        {
            if (child is TNode)
                return true;

            if (child.HasChild<TNode>())
                return true;
        }

        return false;
    }

    public bool HasChild<TNode>(Func<TNode, bool> conditions)
    {
        if (!HasChildren) throw new NotSupportedException();
        foreach (MetadataTreeNodeBase child in Children)
        {
            if (child is TNode node && conditions(node))
                return true;

            if (child.HasChild(conditions))
                return true;
        }

        return false;
    }

    public bool HasChild(IMetadataTreeNode node)
    {
        for (MetadataTreeNodeBase? current = node.Parent; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, this))
                return true;
        }

        return false;
    }

    public bool IsRelated(MetadataTreeNodeBase other)
    {
        if (ReferenceEquals(this, other))
            return true;

        for (MetadataTreeNodeBase? current = Parent; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, other))
                return true;
        }

        for (MetadataTreeNodeBase? current = other.Parent; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, this))
                return true;
        }

        return false;
    }

    public NodeRelationship GetRelationship(MetadataTreeNodeBase other)
    {
        if (ReferenceEquals(this, other))
            return NodeRelationship.Self;

        for (MetadataTreeNodeBase? current = Parent; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, other))
                return NodeRelationship.Descendant;
        }

        for (MetadataTreeNodeBase? current = other.Parent; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, this))
                return NodeRelationship.Ancestor;
        }

        return NodeRelationship.None;
    }

    public virtual bool RemoveChild(MetadataTreeNodeBase metadataTreeNodeBase)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        if (!ChildrenInternal.Remove(metadataTreeNodeBase))
            return false;

        metadataTreeNodeBase.Parent = null;
        return true;
    }

    public virtual void RemoveChildAt(int index)
    {
        ChildrenInternal[index].Parent = null;
        ChildrenInternal.RemoveAt(index);
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

    public virtual bool RemoveChildRecursive(MetadataTreeNodeBase target)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        if (RemoveChild(target))
            return true;

        for (int i = ChildrenInternal.Count - 1; i >= 0; i--)
        {
            if (ChildrenInternal[i].HasChildren && ChildrenInternal[i].RemoveChildRecursive(target))
                return true;
        }

        return false;
    }

    public virtual bool RemoveChild<TMetadataBase>(Func<TMetadataBase, bool> conditions)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        bool result = false;
        for (int i = ChildrenInternal.Count - 1; i >= 0; i--)
        {
            MetadataTreeNodeBase child = ChildrenInternal[i];
            if (child is TMetadataBase metadataBase && conditions(metadataBase))
            {
                ChildrenInternal.RemoveAt(i);
                child.Parent = null;
                result = true;
            }
        }

        return result;
    }

    public virtual bool RemoveChildAndEmptyParent<TMetadataBase>(Func<TMetadataBase, bool> conditions, bool includeSelf)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        bool removed = false;

        for (int i = ChildrenInternal.Count - 1; i >= 0; i--)
        {
            MetadataTreeNodeBase child = ChildrenInternal[i];

            if (child is TMetadataBase metadataBase && conditions(metadataBase))
            {
                ChildrenInternal.RemoveAt(i);
                child.Parent = null;
                removed = true;
                continue;
            }

            if (child.HasChildren)
            {
                if (child.RemoveChildAndEmptyParent(conditions, true))
                    removed = true;
            }
        }

        if (includeSelf && ChildrenInternal.Count == 0)
            RemoveFromParent();

        return removed;
    }

    public virtual bool RemoveChildAndEmptyParent(MetadataTreeNodeBase target, bool includeSelf)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        for (int i = ChildrenInternal.Count - 1; i >= 0; i--)
        {
            MetadataTreeNodeBase child = ChildrenInternal[i];

            if (child == target)
            {
                ChildrenInternal.RemoveAt(i);
                child.Parent = null;

                if (includeSelf && ChildrenInternal.Count == 0)
                    RemoveFromParent();
                return true;
            }

            if (child.HasChildren)
            {
                if (child.RemoveChildAndEmptyParent(target, true))
                {
                    if (includeSelf && ChildrenInternal.Count == 0)
                        RemoveFromParent();
                    return true;
                }
            }
        }

        return false;
    }

    public virtual bool RemoveChild<TMetadataBase, TSubMetadataBase>(Func<TMetadataBase, bool> conditions,
        Func<TSubMetadataBase, bool> subConditions)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        for (int i = ChildrenInternal.Count - 1; i >= 0; i--)
        {
            MetadataTreeNodeBase child = ChildrenInternal[i];
            if (child is TMetadataBase metadataBase && conditions(metadataBase))
            {
                ChildrenInternal.RemoveAt(i);
                child.Parent = null;
                return true;
            }

            if (child.HasChildren && child is TSubMetadataBase subMetadataBase)
            {
                if (!subConditions(subMetadataBase))
                    continue;

                if (child.RemoveChild(conditions, subConditions))
                    return true;
            }
        }

        return false;
    }

    public virtual bool RemoveEmptyParent()
    {
        if (!HasChildren)
            throw new NotSupportedException();

        if (Children.Count != 0)
            return false;

        RemoveFromParent();
        return true;
    }

    public virtual bool RemoveEmptyParentInChild(bool includeSelf)
    {
        if (!HasChildren)
            throw new NotSupportedException();

        bool result = false;
        for (int i = ChildrenInternal.Count - 1; i >= 0; i--)
        {
            MetadataTreeNodeBase child = ChildrenInternal[i];
            if (!child.HasChildren) continue;
            if (child.RemoveEmptyParentInChild(true))
                result = true;
        }

        if (includeSelf && RemoveEmptyParent())
            result = true;

        return result;
    }

    public virtual void ClearChildren()
    {
        foreach (MetadataTreeNodeBase child in ChildrenInternal)
            child.Parent = null;

        ChildrenInternal.Clear();
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

    public IEnumerable<TNode> FindChildren<TNode>()
    {
        foreach (MetadataTreeNodeBase fileMetadataBase in Children)
        {
            if (fileMetadataBase is TNode node)
                yield return node;

            if (!fileMetadataBase.HasChildren) continue;
            foreach (TNode nested in fileMetadataBase.FindChildren<TNode>())
                yield return nested;
        }
    }

    public IEnumerable<TNode> FindChildren<TNode>(Func<TNode, bool> conditions)
    {
        foreach (MetadataTreeNodeBase fileMetadataBase in Children)
        {
            if (fileMetadataBase is TNode node && conditions(node))
                yield return node;

            if (!fileMetadataBase.HasChildren) continue;
            foreach (TNode nested in fileMetadataBase.FindChildren(conditions))
                yield return nested;
        }
    }

    public IEnumerable<TNode> FindChildren<TNode, TSubNode>(Func<TNode, bool> conditions,
        Func<TSubNode, bool> subConditions)
    {
        foreach (MetadataTreeNodeBase fileMetadataBase in Children)
        {
            if (fileMetadataBase is TNode node && conditions(node))
                yield return node;

            if (!fileMetadataBase.HasChildren || fileMetadataBase is not TSubNode subNode || !subConditions(subNode))
                continue;
            foreach (TNode nested in fileMetadataBase.FindChildren(conditions, subConditions))
                yield return nested;
        }
    }

    private AvaloniaList<MetadataTreeNodeBase> InitializeChildren()
    {
        var list = new AvaloniaList<MetadataTreeNodeBase>();
        return list;
    }
}