using System.Collections.Generic;

namespace AutoOrganize.Models.Args;

public readonly struct SelectFilesArgs
{
    public IEnumerable<string>? PickedItems { get; init; } = null;

    public bool CanClearOld { get; init; } = false;

    public SelectFilesArgs()
    {
    }
}