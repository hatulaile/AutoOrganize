using System.Collections.Generic;
using System.Runtime.InteropServices;
using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Models;

[StructLayout(LayoutKind.Auto)]
public readonly struct FileProcessOptions
{
    public MetadataType Type { get; init; }

    public required IEnumerable<string> FilesPaths { get; init; }

    public FileProcessOptions()
    {
    }
}