using System.Linq;
using AutoOrganize.ViewModels;
using Avalonia;
using StaticViewLocator;

namespace AutoOrganize.ViewLocators;

[StaticViewLocator]
public partial class ViewLocator : IViewLocator
{
    public static IViewLocator DefaultViewLocator
    {
        get
        {
            if (field is not null)
                return field;

            ViewLocator viewLocator = Application.Current!.DataTemplates.Cast<ViewLocator>().First();
            return field = viewLocator;
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}