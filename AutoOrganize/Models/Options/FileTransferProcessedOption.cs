using System.Collections.Generic;
using AutoOrganize.Library.Models;

namespace AutoOrganize.Models.Options;

public readonly record struct FileTransferProcessedOption(IEnumerable<FileMetadataEntry> FileMetadataEntries);