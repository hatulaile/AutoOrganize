using AutoOrganize.Library.Extensions;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.FileTransfers;
using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.FileTransferServices;
using AutoOrganize.Library.Services.Observers;
using AutoOrganize.Library.Services.PathNameGenerators;
using AutoOrganize.Library.Services.PathNameGenerators.Configs;
using AutoOrganize.Library.Services.PathNameGenerators.Options;

namespace AutoOrganize.Library.Services.FileTransferBatchServices;

public sealed class FileTransferBatchService : IFileTransferBatchService
{
    private readonly IFileTransferService _fileTransferService;
    private readonly IPathNameGenerator _pathNameGenerator;
    private readonly IFileConfigManager _fileConfigManager;

    private FileTransferConfig FileTransferConfig =>
        _fileConfigManager.GetRequiredConfig<FileTransferConfig>();

    private FileNameGeneratorConfig FileNameGeneratorConfig =>
        _fileConfigManager.GetRequiredConfig<FileNameGeneratorConfig>();

    // progress 不一定会在传入线程上调用
    public async Task<FileTransferBatchResult> ProcessFilesAsync(IEnumerable<FileMetadataEntry> fileMetadataEntries,
        IProcessObserver<FileTransferBatchInfo, FileTransferBatchResult>? progress = null,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        FileTransferOptions fileTransferOptions = FileTransferConfig.ToOption();
        FileNameGenerationOptions fileNameGenerationOptions = FileNameGeneratorConfig.ToOptions();
        string? directoryPath = null;
        var result = new FileTransferBatchResult();

        foreach (var fileMetadataEntry in fileMetadataEntries)
        {
            directoryPath ??= FileTransferConfig.OutputDirectory.StartsWith("./")
                ? Path.Combine(
                    Path.GetPathRoot(fileMetadataEntry.FilePath) ?? throw new Exception("Get Root Failed"),
                    FileTransferConfig.OutputDirectory.Remove(0, 2))
                : FileTransferConfig.OutputDirectory;

            token.ThrowIfCancellationRequested();
            try
            {
                string path = GetOutputFilePath(fileMetadataEntry, directoryPath, fileNameGenerationOptions);
                string directoryName = Path.GetDirectoryName(path) ?? throw new Exception("Get Directory Failed");
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);

                await _fileTransferService
                    .TransferFileAsync(
                        new FileTransferEntry(fileMetadataEntry.FilePath, path),
                        fileTransferOptions, token).ConfigureAwait(false);

                result.Succeed++;
                progress?.OnSuccess(new FileTransferBatchInfo(fileMetadataEntry));
            }
            catch (Exception e)
            {
                result.Failed++;
                progress?.OnFailure(e);
            }
        }

        progress?.OnCompleted(result);
        return result;
    }

    //如果过于复杂考虑再抽成一个 Service
    private string GetOutputFilePath(FileMetadataEntry fileMetadataEntry, string directoryPath,
        FileNameGenerationOptions options)
    {
        var path = fileMetadataEntry.Metadata switch
        {
            EpisodeMetadata episodeMetadata => GetOutputTvFilePath(episodeMetadata, fileMetadataEntry.FilePath,
                options.TvFileNameGenerationOptions),
            MovieMetadata movieMetadata => GetOutputMovieFilePath(movieMetadata, fileMetadataEntry.FilePath,
                options.MovieFileNameGenerationOptions),
            _ => throw new ArgumentException($"{fileMetadataEntry.Metadata} is not a file metadata")
        };


        return Path.Combine(directoryPath, path);

        string GetOutputTvFilePath(EpisodeMetadata episodeMetadata, string filePath,
            TvFileNameGenerationOptions tvFileNameGenerationOptions)
        {
            string seriesPath = episodeMetadata.Series is not null
                ? _pathNameGenerator.GetTvSeriesFileName(episodeMetadata.Series, tvFileNameGenerationOptions)
                : "Unknown";
            string seasonPath = episodeMetadata.Season is not null
                ? _pathNameGenerator.GetTvSeasonFileName(episodeMetadata.Season, tvFileNameGenerationOptions)
                : "Unknown";
            string episodePath =
                _pathNameGenerator.GetTvEpisodeFileName(filePath, episodeMetadata, tvFileNameGenerationOptions);

            return Path.Combine(directoryPath, seriesPath, seasonPath, episodePath);
        }

        string GetOutputMovieFilePath(MovieMetadata episodeMetadata, string filePath,
            MovieFileNameGenerationOptions movieFileNameGenerationOptions)
        {
            string movieFileName =
                _pathNameGenerator.GetMovieFileName(filePath, episodeMetadata, movieFileNameGenerationOptions);
            if (!FileTransferConfig.IsCreateMovieFolder)
                return movieFileName;

            string movieDirectory =
                _pathNameGenerator.GetMovieFolderName(episodeMetadata, movieFileNameGenerationOptions);
            return Path.Combine(movieDirectory, movieFileName);
        }
    }

    public FileTransferBatchService(IFileTransferService fileTransferService, IPathNameGenerator pathNameGenerator,
        IFileConfigManager fileConfigManager)
    {
        _fileTransferService = fileTransferService;
        _pathNameGenerator = pathNameGenerator;
        _fileConfigManager = fileConfigManager;
        fileConfigManager.LoadConfigOrNew<FileTransferConfig>();
        fileConfigManager.LoadConfigOrNew<FileNameGeneratorConfig>();
    }
}