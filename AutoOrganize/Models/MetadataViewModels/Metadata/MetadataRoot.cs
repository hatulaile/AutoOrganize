using System;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Models.MetadataViewModels.FileSystem;

namespace AutoOrganize.Models.MetadataViewModels.Metadata;

public sealed class MetadataRoot : FileMetadataBase, IMetadataTreeRoot
{
    public override string Title => string.Empty;

    public override FileMetadataType Type => FileMetadataType.Directory;

    public override bool HasChildren => true;

    public void AddTransferFile(string filePath, string outputPath, MetadataBase metadata)
    {
        var model = new TransferFileModel(filePath, outputPath);
        AddOrGetMetadata(metadata).AddChild(model);
    }

    public void AddFailedTransferFile(string filePath, string? outputPath, MetadataBase metadata, Exception exception)
    {
        var model = new FailedTransferFileModel(filePath, outputPath, exception);
        AddOrGetMetadata(metadata).AddChild(model);
    }

    public void AddFile(MetadataBase metadata, string filePath)
    {
        var fileModel = new FileModel(filePath);
        AddOrGetMetadata(metadata).AddChild(fileModel);
    }

    public  FileMetadataBase AddOrGetMetadata(MetadataBase metadata)
    {
        return metadata switch
        {
            MovieMetadata movieMetadata => AddOrGetMovie(movieMetadata),
            EpisodeMetadata episodeMetadata => AddOrGetEpisode(episodeMetadata),
            _ => throw new Exception($"Unknown metadata type: {metadata.GetType()}")
        };
    }

    public FileEpisodeMetadata AddOrGetEpisode(EpisodeMetadata episodeMetadata)
    {
        var fileEpisodeMetadata = GetChildren<FileEpisodeMetadata, IFileMetadata<MetadataBase>>
        (x => x.Metadata == episodeMetadata,
            x => x.Metadata == episodeMetadata.Season || x.Metadata == episodeMetadata.Series);
        if (fileEpisodeMetadata != null)
            return fileEpisodeMetadata;

        FileSeasonMetadata fileSeasonMetadata = AddOrGetSeason(episodeMetadata.Season ?? throw new Exception("Season is null"));
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
        var fileSeasonMetadata = GetChildren<FileSeasonMetadata, IFileMetadata<MetadataBase>>
            (x => x.Metadata == seasonMetadata, x => x.Metadata == seasonMetadata.Series);
        if (fileSeasonMetadata != null)
            return fileSeasonMetadata;

        var fileSeriesMetadata = AddOrGetSeries(seasonMetadata.Series ?? throw new Exception("Series is null"));
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