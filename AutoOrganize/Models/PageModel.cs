using System;

namespace AutoOrganize.Models;

public readonly struct PageModel : IEquatable<PageModel>
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
        return Icon == other.Icon && Title == other.Title && ViewModelType == other.ViewModelType;
    }

    public override bool Equals(object? obj)
    {
        return obj is PageModel other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Icon, Title, ViewModelType);
    }
}