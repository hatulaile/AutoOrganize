using System.Runtime.InteropServices;

namespace AutoOrganize.Library.Services.PathNameGenerators.Options;

[StructLayout(LayoutKind.Auto)]
public struct MovieFileNameGenerationOptions
{
    public static MovieFileNameGenerationOptions Empty { get; } = new();

    public string? MoviePattern { get; set; }

    public string? MovieFolderPattern { get; set; }
}