using System;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.MetadataNodes.FileSystem;

namespace AutoOrganize.Models.MetadataNodes.Metadata;

public sealed class MetadataTreeRoot : MetadataTreeNodeBase, IMetadataTreeRoot
{
    public override string Title => string.Empty;

    public override MetadataNodeType NodeType => MetadataNodeType.Directory;

    public override bool HasChildren => true;

    public void AddTransferFile(string filePath, string outputPath, MetadataBase metadata)
    {
        var model = new TransferredFileNode(filePath, outputPath);
        AddOrGetMetadata(metadata).AddChild(model);
    }

    public void AddFailedTransferFile(string filePath, string? outputPath, MetadataBase metadata, Exception exception)
    {
        var model = new FailedTransferFileNode(filePath, outputPath, exception);
        AddOrGetMetadata(metadata).AddChild(model);
    }

    public void AddFile(MetadataBase metadata, string filePath)
    {
        var fileModel = new SourceFileNode(filePath);
        AddOrGetMetadata(metadata).AddChild(fileModel);
    }

    public  MetadataTreeNodeBase AddOrGetMetadata(MetadataBase metadata)
    {
        return metadata switch
        {
            MovieMetadata movieMetadata => AddOrGetMovie(movieMetadata),
            EpisodeMetadata episodeMetadata => AddOrGetEpisode(episodeMetadata),
            _ => throw new Exception($"Unknown metadataTreeNode nodeType: {metadata.GetType()}")
        };
    }

    public EpisodeMetadataTreeNode AddOrGetEpisode(EpisodeMetadata episodeMetadata)
    {
        var fileEpisodeMetadata = GetChildren<EpisodeMetadataTreeNode, IFileMetadata<MetadataBase>>
        (x => x.Metadata == episodeMetadata,
            x => x.Metadata == episodeMetadata.Season || x.Metadata == episodeMetadata.Series);
        if (fileEpisodeMetadata != null)
            return fileEpisodeMetadata;

        SeasonMetadataTreeNode seasonMetadataTreeNode = AddOrGetSeason(episodeMetadata.Season ?? throw new Exception("Season is null"));
        var newEpisode = new EpisodeMetadataTreeNode(episodeMetadata);
        int index = seasonMetadataTreeNode.IndexOfChild(x =>
        {
            if (x is IFileMetadata<EpisodeMetadata> metadata)
            {
                return episodeMetadata.EpisodeNumber < metadata.Metadata.EpisodeNumber;
            }

            return false;
        });
        if (index > -1) seasonMetadataTreeNode.InsertChild(index, newEpisode);
        else seasonMetadataTreeNode.AddChild(newEpisode);
        return newEpisode;
    }

    public SeasonMetadataTreeNode AddOrGetSeason(SeasonMetadata seasonMetadata)
    {
        var fileSeasonMetadata = GetChildren<SeasonMetadataTreeNode, IFileMetadata<MetadataBase>>
            (x => x.Metadata == seasonMetadata, x => x.Metadata == seasonMetadata.Series);
        if (fileSeasonMetadata != null)
            return fileSeasonMetadata;

        var fileSeriesMetadata = AddOrGetSeries(seasonMetadata.Series ?? throw new Exception("Series is null"));
        var newSeason = new SeasonMetadataTreeNode(seasonMetadata);
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

    public SeriesMetadataTreeNode AddOrGetSeries(SeriesMetadata seriesMetadata)
    {
        var fileSeriesMetadata = GetChildren<SeriesMetadataTreeNode>(x => x.Metadata == seriesMetadata);
        if (fileSeriesMetadata != null)
            return fileSeriesMetadata;

        var newSeries = new SeriesMetadataTreeNode(seriesMetadata);
        AddChild(newSeries);
        return newSeries;
    }

    public MovieMetadataTreeNode AddOrGetMovie(MovieMetadata movieMetadata)
    {
        var fileMovieMetadata = GetChildren<MovieMetadataTreeNode>(x => x.Metadata == movieMetadata);
        if (fileMovieMetadata != null)
            return fileMovieMetadata;

        var newMovie = new MovieMetadataTreeNode(movieMetadata);
        AddChild(newMovie);
        return newMovie;
    }
}