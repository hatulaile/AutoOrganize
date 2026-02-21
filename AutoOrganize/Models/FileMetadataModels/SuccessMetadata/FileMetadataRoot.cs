using System;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;

namespace AutoOrganize.Models.FileMetadataModels.SuccessMetadata;

public sealed class FileMetadataRoot : FileMetadataBase, IMetadataTreeRoot
{
    public override string Title => string.Empty;

    public override FileMetadataType Type => FileMetadataType.Directory;

    public override bool HasChildren => true;

    public void AddFile(FileMetadataProcessingResult processingResult)
    {
        ArgumentNullException.ThrowIfNull(processingResult.Metadata);

        var metadata = new FileMetadata(processingResult.FilePath);
        if (processingResult.Metadata is MovieMetadata movieMetadata)
        {
            AddOrGetMovie(movieMetadata).AddChild(metadata);
            return;
        }

        if (processingResult.Metadata is EpisodeMetadata episodeMetadata)
        {
            AddOrGetEpisode(episodeMetadata).AddChild(metadata);
            return;
        }

        throw new Exception($"Unknown metadata type: {processingResult.Metadata.GetType()}");
    }

    public FileEpisodeMetadata AddOrGetEpisode(EpisodeMetadata episodeMetadata)
    {
        ArgumentNullException.ThrowIfNull(episodeMetadata.Season);
        ArgumentNullException.ThrowIfNull(episodeMetadata.Series);

        var fileEpisodeMetadata = GetChildren<FileEpisodeMetadata, IFileMetadata<MetadataBase>>
        (x => x.Metadata == episodeMetadata,
            x => x.Metadata == episodeMetadata.Season || x.Metadata == episodeMetadata.Series);
        if (fileEpisodeMetadata != null)
            return fileEpisodeMetadata;

        FileSeasonMetadata fileSeasonMetadata = AddOrGetSeason(episodeMetadata.Season);
        var newEpisode = new FileEpisodeMetadata(episodeMetadata);
        int index = fileSeasonMetadata.IndexOfChild(x =>
        {
            if (x is IFileMetadata<EpisodeMetadata> metadata)
            {
                return episodeMetadata.EpisodeNumber < metadata.Metadata.EpisodeNumber;
            }

            return false;
        });
        if (index > -1) fileSeasonMetadata.InsertChild(index, newEpisode);
        else fileSeasonMetadata.AddChild(newEpisode);
        return newEpisode;
    }

    public FileSeasonMetadata AddOrGetSeason(SeasonMetadata seasonMetadata)
    {
        ArgumentNullException.ThrowIfNull(seasonMetadata.Series);
        var fileSeasonMetadata = GetChildren<FileSeasonMetadata, IFileMetadata<MetadataBase>>
            (x => x.Metadata == seasonMetadata, x => x.Metadata == seasonMetadata.Series);
        if (fileSeasonMetadata != null)
            return fileSeasonMetadata;

        var fileSeriesMetadata = AddOrGetSeries(seasonMetadata.Series);
        var newSeason = new FileSeasonMetadata(seasonMetadata);
        int index = fileSeriesMetadata.IndexOfChild(x =>
        {
            if (x is IFileMetadata<SeasonMetadata> metadata)
            {
                return seasonMetadata.SeasonNumber < metadata.Metadata.SeasonNumber;
            }

            return false;
        });
        if (index > -1) fileSeriesMetadata.InsertChild(index, newSeason);
        else fileSeriesMetadata.AddChild(newSeason);
        return newSeason;
    }

    public FileSeriesMetadata AddOrGetSeries(SeriesMetadata seriesMetadata)
    {
        var fileSeriesMetadata = GetChildren<FileSeriesMetadata>(x => x.Metadata == seriesMetadata);
        if (fileSeriesMetadata != null)
            return fileSeriesMetadata;

        var newSeries = new FileSeriesMetadata(seriesMetadata);
        AddChild(newSeries);
        return newSeries;
    }

    public FileMovieMetadata AddOrGetMovie(MovieMetadata movieMetadata)
    {
        var fileMovieMetadata = GetChildren<FileMovieMetadata>(x => x.Metadata == movieMetadata);
        if (fileMovieMetadata != null)
            return fileMovieMetadata;

        var newMovie = new FileMovieMetadata(movieMetadata);
        AddChild(newMovie);
        return newMovie;
    }
}