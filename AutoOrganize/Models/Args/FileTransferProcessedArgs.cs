using System.Collections.Generic;
using AutoOrganize.Library.Models;

namespace AutoOrganize.Models.Args;

public readonly record struct FileTransferProcessedArgs(IEnumerable<FileMetadataEntry> FileMetadataEntries);