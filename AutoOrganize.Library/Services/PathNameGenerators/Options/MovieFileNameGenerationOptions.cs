using System.Runtime.InteropServices;

namespace AutoOrganize.Library.Services.PathNameGenerators.Options;

[StructLayout(LayoutKind.Auto)]
public readonly struct MovieFileNameGenerationOptions
{
    public static MovieFileNameGenerationOptions Empty { get; } = new();

    public string? MoviePattern { get; init; }

    public string? MovieFolderPattern { get; init; }

    public MovieFileNameGenerationOptions()
    {
    }
}