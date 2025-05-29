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
using Microsoft.Extensions.Logging;

namespace AutoOrganize.Library.Services.FileTransferBatchServices;

public sealed class FileTransferBatchService : IFileTransferBatchService
{
    private readonly IFileTransferService _fileTransferService;
    private readonly IFileNameGenerator _fileNameGenerator;
    private readonly IFileConfigManager _fileConfigManager;
    private readonly ILogger<FileTransferBatchService> _logger;

    private FileTransferConfig FileTransferConfig =>
        _fileConfigManager.GetRequiredConfig<FileTransferConfig>();

    private FileNameGeneratorConfig FileNameGeneratorConfig =>
        _fileConfigManager.GetRequiredConfig<FileNameGeneratorConfig>();

    // progress 不一定会在传入线程上调用
    public async Task<FileTransferBatchResult> ProcessFilesAsync(IEnumerable<FileMetadataEntry> fileMetadataEntries,
        IProcessObserver<FileTransferBatchInfo, FileTransferBatchResult, FileTransferBatchErrorInfo>? progress = null,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        int succeed = 0;
        int failed = 0;

        FileTransferOptions fileTransferOptions = FileTransferConfig.ToOption();
        string? directoryPath = null;

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
                string path = GetOutputFilePath(fileMetadataEntry, directoryPath, FileNameGeneratorConfig);
                try
                {
                    await _fileTransferService
                        .TransferFileAsync(new FileTransferEntry(fileMetadataEntry.FilePath, path),
                            fileTransferOptions, token).ConfigureAwait(false);

                    _logger.LogDebug("文件 {file} 至 {output} 处理成功", fileMetadataEntry.FilePath,
                        fileMetadataEntry.Metadata);
                    succeed++;
                    progress?.OnSuccess(
                        new FileTransferBatchInfo(fileMetadataEntry.FilePath, path, fileMetadataEntry.Metadata));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    failed++;
                    _logger.LogError(e, "文件 {file} 至 {output} 失败",
                        fileMetadataEntry.FilePath, path);
                    progress?.OnFailure(new FileTransferBatchErrorInfo(fileMetadataEntry.FilePath, path,
                        fileMetadataEntry.Metadata, e));
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                failed++;
                _logger.LogError(e, "文件 {file} 处理失败", fileMetadataEntry.FilePath);
                progress?.OnFailure(new FileTransferBatchErrorInfo(fileMetadataEntry.FilePath, null,
                    fileMetadataEntry.Metadata, e));
            }
        }

        _logger.LogInformation("文件传输处理完成，成功: {Succeed}, 失败: {Failed}", succeed, failed);
        var result = new FileTransferBatchResult(succeed, failed);
        progress?.OnCompleted(result);
        return result;
    }

    //如果过于复杂考虑再抽成一个 Service
    private string GetOutputFilePath(FileMetadataEntry fileMetadataEntry, string directoryPath,
        FileNameGeneratorConfig fileNameGeneratorConfig)
    {
        var path = fileMetadataEntry.Metadata switch
        {
            EpisodeMetadata episodeMetadata => GetOutputTvFilePath(episodeMetadata, fileMetadataEntry.FilePath,
                fileNameGeneratorConfig.TvFileNameGenerationConfig),
            MovieMetadata movieMetadata => GetOutputMovieFilePath(movieMetadata, fileMetadataEntry.FilePath,
                fileNameGeneratorConfig.MovieFileNameGeneratorConfig),
            _ => throw new ArgumentException($"{fileMetadataEntry.Metadata} is not a file metadata")
        };


        return Path.Combine(directoryPath, path);

        string GetOutputTvFilePath(EpisodeMetadata episodeMetadata, string filePath,
            TvFileNameGenerationConfig config)
        {
            string seriesPath = episodeMetadata.Series is not null
                ? _fileNameGenerator.GetTvSeriesFileName(episodeMetadata.Series, config.SeriesMetadataFolderPattern)
                : "Unknown";
            string seasonPath = episodeMetadata.Season is not null
                ? _fileNameGenerator.GetTvSeasonFileName(episodeMetadata.Season, config.SeasonMetadataFolderPattern)
                : "Unknown";
            string episodePath =
                _fileNameGenerator.GetTvEpisodeFileName(filePath, episodeMetadata, config.EpisodeNamePattern);

            return Path.Combine(directoryPath, seriesPath, seasonPath, episodePath);
        }

        string GetOutputMovieFilePath(MovieMetadata episodeMetadata, string filePath,
            MovieFileNameGeneratorConfig config)
        {
            string movieFileName =
                _fileNameGenerator.GetMovieFileName(filePath, episodeMetadata, config.MoviePattern);
            if (!FileTransferConfig.IsCreateMovieFolder)
                return movieFileName;

            string movieDirectory =
                _fileNameGenerator.GetMovieFolderName(episodeMetadata, config.MovieFolderPattern);
            return Path.Combine(movieDirectory, movieFileName);
        }
    }

    public FileTransferBatchService(IFileTransferService fileTransferService, IFileNameGenerator fileNameGenerator,
        IFileConfigManager fileConfigManager, ILogger<FileTransferBatchService> logger)
    {
        _fileTransferService = fileTransferService;
        _fileNameGenerator = fileNameGenerator;
        _fileConfigManager = fileConfigManager;
        _logger = logger;
        fileConfigManager.GetConfigOrLoad<FileTransferConfig>();
        fileConfigManager.GetConfigOrLoad<FileNameGeneratorConfig>();
    }
}