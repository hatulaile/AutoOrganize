using System.Runtime.InteropServices;

namespace AutoOrganize.Library.Services.PathNameGenerators.Options;

[StructLayout(LayoutKind.Auto)]
public struct FileNameGenerationOptions
{
    public TvFileNameGenerationOptions TvFileNameGenerationOptions { get; set; }

    public MovieFileNameGenerationOptions MovieFileNameGenerationOptions { get; set; }
}