using System.Collections.Generic;
using System.Runtime.InteropServices;
using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Models;

[StructLayout(LayoutKind.Auto)]
public struct FileProcessOptions
{
    public MetadataType Type { get; set; }

    public IEnumerable<string> FilesPaths { get; set; }

    public FileProcessOptions(MetadataType type, IEnumerable<string> filesPaths)
    {
        Type = type;
        FilesPaths = filesPaths;
    }
}