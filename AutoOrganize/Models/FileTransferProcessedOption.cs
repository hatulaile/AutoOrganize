using System.Collections.Generic;
using AutoOrganize.Library.Models;

namespace AutoOrganize.Models;

public readonly record struct FileTransferProcessedOption(IEnumerable<FileMetadataEntry> FileMetadataEntries);