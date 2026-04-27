using System.Runtime.InteropServices;

namespace AutoOrganize.Library.Services.PathNameGenerators.Options;

[StructLayout(LayoutKind.Auto)]
public readonly struct TvFileNameGenerationOptions
{
    public static TvFileNameGenerationOptions Empty { get; } = new TvFileNameGenerationOptions();

    public string? SeriesMetadataFolderPattern { get; init; }

    public string? SeasonMetadataFolderPattern { get; init; }

    public string? EpisodeNamePattern { get; init; }

    public TvFileNameGenerationOptions()
    {
    }
}