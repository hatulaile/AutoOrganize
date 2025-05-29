using System;
using System.Collections.Generic;

namespace AutoOrganize.Models;

public interface IPageModel
{
    string Icon { get; }

    string Title { get; }

    Type? ViewModelType { get; }

    bool IsSeparator => false;

    IReadOnlyList<IPageModel> Children => [];
}

public readonly struct PageModel : IPageModel, IEquatable<PageModel>
{
    public string Icon { get; }

    public string Title { get; }

    public Type ViewModelType { get; }

    public PageModel(string title, string icon, Type viewModelType)
    {
        Title = title;
        Icon = icon;
        ViewModelType = viewModelType;
    }

    public bool Equals(PageModel other)
    {
        return ViewModelType == other.ViewModelType;
    }

    public override bool Equals(object? obj)
    {
        return obj is PageModel other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Icon, Title, ViewModelType);
    }

    public static bool operator ==(PageModel left, PageModel right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PageModel left, PageModel right)
    {
        return !(left == right);
    }
}

public readonly struct PageModel<TViewModel> : IPageModel, IEquatable<PageModel<TViewModel>>
    where TViewModel : class
{
    public string Icon { get; }

    public string Title { get; }

    public Type ViewModelType => typeof(TViewModel);

    public PageModel(string title, string icon)
    {
        Title = title;
        Icon = icon;
    }

    public bool Equals(PageModel<TViewModel> other)
    {
        return other.ViewModelType == ViewModelType;
    }

    public override bool Equals(object? obj)
    {
        return obj is PageModel<TViewModel> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Icon, Title);
    }

    public static bool operator ==(PageModel<TViewModel> left, PageModel<TViewModel> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PageModel<TViewModel> left, PageModel<TViewModel> right)
    {
        return !(left == right);
    }
}

public readonly struct SeparatorPageModel : IPageModel, IEquatable<SeparatorPageModel>
{
    public string Icon => string.Empty;

    public string Title => string.Empty;

    public Type? ViewModelType => null;

    public bool IsSeparator => true;

    public bool Equals(SeparatorPageModel other) => true;

    public override bool Equals(object? obj) => obj is SeparatorPageModel;

    public override int GetHashCode() => nameof(SeparatorPageModel).GetHashCode();

    public static bool operator ==(SeparatorPageModel left, SeparatorPageModel right) => true;

    public static bool operator !=(SeparatorPageModel left, SeparatorPageModel right) => false;
}

public readonly struct CategoryPageModel : IPageModel, IEquatable<CategoryPageModel>
{
    public string Icon { get; }

    public string Title { get; }

    public Type? ViewModelType => null;

    public IReadOnlyList<IPageModel> Children { get; }

    public CategoryPageModel(string title, string icon, params IPageModel[] children)
    {
        Title = title;
        Icon = icon;
        Children = children;
    }

    public CategoryPageModel(string title, string icon, IReadOnlyList<IPageModel> children)
    {
        Title = title;
        Icon = icon;
        Children = children;
    }

    public bool Equals(CategoryPageModel other)
    {
        return Title == other.Title && Icon == other.Icon;
    }

    public override bool Equals(object? obj)
    {
        return obj is CategoryPageModel other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Icon, Title);
    }

    public static bool operator ==(CategoryPageModel left, CategoryPageModel right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CategoryPageModel left, CategoryPageModel right)
    {
        return !(left == right);
    }
}