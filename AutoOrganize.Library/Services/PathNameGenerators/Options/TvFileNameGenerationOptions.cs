using System.Runtime.InteropServices;

namespace AutoOrganize.Library.Services.PathNameGenerators.Options;

[StructLayout(LayoutKind.Auto)]
public struct TvFileNameGenerationOptions
{
    public static TvFileNameGenerationOptions Empty { get; } = new();

    public string? SeriesMetadataFolderPattern { get; set; }

    public string? SeasonMetadataFolderPattern { get; set; }

    public string? EpisodeNamePattern { get; set; }
}