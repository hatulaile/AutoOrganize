using System.Collections.Generic;
using AutoOrganize.Library.Models;

namespace AutoOrganize.Models;

public sealed record FileTransferProcessedOption(IEnumerable<FileMetadataEntry> FileMetadataEntries);