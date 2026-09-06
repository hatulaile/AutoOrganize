using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Collections;

namespace AutoOrganize.Models.MetadataNodes.Abstractions;

public interface IMetadataTreeNode : INotifyPropertyChanged
{
    string? Title { get; }

    MetadataNodeType NodeType { get; }

    bool HasChildren { get; }

    MetadataTreeNodeBase? Parent { get; }

    IAvaloniaReadOnlyList<MetadataTreeNodeBase> Children { get; }

    void AddChild(MetadataTreeNodeBase metadataTreeNodeBase);

    void InsertChild(int index, MetadataTreeNodeBase metadataTreeNodeBase);

    bool RemoveFromParent();

    MetadataTreeNodeBase? FindRootParent();

    TMetadataBase? FindParent<TMetadataBase>();

    TMetadataBase? FindParent<TMetadataBase>(Func<TMetadataBase, bool> conditions);

    bool HasParent<TMetadataBase>();

    bool HasParent<TMetadataBase>(Func<TMetadataBase, bool> conditions);

    bool HasParent(IMetadataTreeNode node);

    bool HasChild<TNode>();

    bool HasChild<TNode>(Func<TNode, bool> conditions);

    bool HasChild(IMetadataTreeNode node);

    bool IsRelated(MetadataTreeNodeBase other);

    NodeRelationship GetRelationship(MetadataTreeNodeBase other);

    bool RemoveChild(MetadataTreeNodeBase metadataTreeNodeBase);

    void RemoveChildAt(int index);

    int IndexOfChild(Func<MetadataTreeNodeBase, bool> conditions);

    int IndexOfChild(MetadataTreeNodeBase metadataTreeNodeBase);

    bool RemoveChildRecursive(MetadataTreeNodeBase target);

    bool RemoveChild<TMetadataBase>(Func<TMetadataBase, bool> conditions);

    bool RemoveChildAndEmptyParent<TMetadataBase>(Func<TMetadataBase, bool> conditions, bool includeSelf);

    bool RemoveChildAndEmptyParent(MetadataTreeNodeBase target, bool includeSelf);

    bool RemoveChild<TMetadataBase, TSubMetadataBase>(Func<TMetadataBase, bool> conditions,
        Func<TSubMetadataBase, bool> subConditions);

    bool RemoveEmptyParent();

    bool RemoveEmptyParentInChild(bool includeSelf);

    void ClearChildren();

    TMetadataBase? GetChildren<TMetadataBase>(Func<TMetadataBase, bool> conditions);

    TMetadataBase? GetChildren<TMetadataBase, TSubMetadataBase>(Func<TMetadataBase, bool> conditions,
        Func<TSubMetadataBase, bool> subConditions);

    IEnumerable<TNode> FindChildren<TNode>();

    IEnumerable<TNode> FindChildren<TNode>(Func<TNode, bool> conditions);

    IEnumerable<TNode> FindChildren<TNode, TSubNode>(Func<TNode, bool> conditions,
        Func<TSubNode, bool> subConditions);
}