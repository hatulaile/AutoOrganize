using System.Runtime.InteropServices;

namespace AutoOrganize.Library.Services.PathNameGenerators.Options;

[StructLayout(LayoutKind.Auto)]
public readonly struct FileNameGenerationOptions
{
    public TvFileNameGenerationOptions TvFileNameGenerationOptions { get; init; }

    public MovieFileNameGenerationOptions MovieFileNameGenerationOptions { get; init; }

    public FileNameGenerationOptions()
    {
    }
}