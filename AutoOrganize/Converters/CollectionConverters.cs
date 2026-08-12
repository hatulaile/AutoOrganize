using System.Collections;
using Avalonia.Data.Converters;

namespace AutoOrganize.Converters;

public static class CollectionConverters
{
    public static FuncValueConverter<ICollection?, bool> CollectionCountIsOneConverter => field ??=
        new FuncValueConverter<ICollection?, bool>(collection => collection is { Count: 1 });

    public static FuncValueConverter<ICollection?, bool> CollectionCountIsMoreThanOneConverter => field ??=
        new FuncValueConverter<ICollection?, bool>(collection => collection is { Count: > 1 });
}