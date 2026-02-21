using AutoOrganize.Library.Services.PathNameGenerators.Configs;
using AutoOrganize.Library.Services.PathNameGenerators.Options;

namespace AutoOrganize.Library.Extensions;

public static class FileNameGeneratorConfigExtensions
{
    extension(FileNameGeneratorConfig config)
    {
        public FileNameGenerationOptions ToOptions()
        {
            return new FileNameGenerationOptions
            {
                TvFileNameGenerationOptions = config.TvFileNameGenerationConfig.ToOptions(),
                MovieFileNameGenerationOptions = config.MovieFileNameGeneratorConfig.ToOptions()
            };
        }
    }

    extension(TvFileNameGenerationConfig config)
    {
        public TvFileNameGenerationOptions ToOptions()
        {
            return new TvFileNameGenerationOptions
            {
                SeriesMetadataFolderPattern = config.SeriesMetadataFolderPattern,
                SeasonMetadataFolderPattern = config.SeasonMetadataFolderPattern,
                EpisodeNamePattern = config.EpisodeNamePattern
            };
        }
    }

    extension(MovieFileNameGeneratorConfig config)
    {
        public MovieFileNameGenerationOptions ToOptions()
        {
            return new MovieFileNameGenerationOptions
            {
                MoviePattern = config.MoviePattern,
                MovieFolderPattern = config.MovieFolderPattern
            };
        }
    }
}