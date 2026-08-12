using System;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Movie;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Models.MetadataNodes.FileSystem;

namespace AutoOrganize.Models;

public sealed record MovieIdentifyResult(SourceFileNode SourceFile, MovieMetadata? Metadata, Exception? Error);

public sealed record TvIdentifyResult(SourceFileNode SourceFile, EpisodeMetadata? Metadata, Exception? Error);

public sealed record FailedMovieIdentifyResult(FailedFileNode FailedFile, MovieMetadata? Metadata, Exception? Error);

public sealed record FailedTvIdentifyResult(FailedFileNode FailedFile, EpisodeMetadata? Metadata, Exception? Error);