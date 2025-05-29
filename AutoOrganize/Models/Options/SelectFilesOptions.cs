using System.Collections.Generic;

namespace AutoOrganize.Models.Options;

public readonly struct SelectFilesOptions
{
    public IEnumerable<string>? PickedItems { get; init; } = null;

    public bool CanClearOld { get; init; } = false;

    public SelectFilesOptions()
    {
    }
}