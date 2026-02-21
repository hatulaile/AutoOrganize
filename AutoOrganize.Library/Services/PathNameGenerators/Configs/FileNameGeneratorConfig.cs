using AutoOrganize.Library.Services.Config;

namespace AutoOrganize.Library.Services.PathNameGenerators.Configs;

public sealed class FileNameGeneratorConfig : IConfig<FileNameGeneratorConfig>
{
    public TvFileNameGenerationConfig TvFileNameGenerationConfig { get; set; } = new();

    public MovieFileNameGeneratorConfig MovieFileNameGeneratorConfig { get; set; } = new();

    public static void Copy(FileNameGeneratorConfig target, FileNameGeneratorConfig source)
    {
        target.TvFileNameGenerationConfig = source.TvFileNameGenerationConfig;
        target.MovieFileNameGeneratorConfig = source.MovieFileNameGeneratorConfig;
    }
}