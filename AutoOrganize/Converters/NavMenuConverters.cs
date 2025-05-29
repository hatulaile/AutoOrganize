using AutoOrganize.Models;
using Avalonia.Data.Converters;

namespace AutoOrganize.Converters;

public static class NavMenuConverters
{
    public static FuncValueConverter<IPageModel, int> HeaderPageModelToGridColumnConverter =>
        field ??= new FuncValueConverter<IPageModel, int>(model => string.IsNullOrEmpty(model?.Icon) ? 0 : 1);

    public static FuncValueConverter<IPageModel, int> HeaderPageModelToGridColumnSpanConverter =>
        field ??= new FuncValueConverter<IPageModel, int>(model =>
        {
            bool hasIcon = !string.IsNullOrEmpty(model?.Icon);
            bool hasChildren = model?.Children.Count > 0;
            if (!hasIcon && !hasChildren)
                return 3;
            return hasIcon && hasChildren ? 1 : 2;
        });
}