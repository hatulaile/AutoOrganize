using System;

namespace AutoOrganize.Models;

public sealed class PageModel
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
}