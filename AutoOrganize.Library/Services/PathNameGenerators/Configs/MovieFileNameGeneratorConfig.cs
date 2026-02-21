namespace AutoOrganize.Library.Services.PathNameGenerators.Configs;

public class MovieFileNameGeneratorConfig
{
    public string MoviePattern { get; set; } = PathNameGenerator.MOVIE_PATTERN;

    public string MovieFolderPattern { get; set; } = PathNameGenerator.MOVIE_FOLDER_PATTERN;
}