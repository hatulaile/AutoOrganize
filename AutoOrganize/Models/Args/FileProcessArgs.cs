using System.Collections.Generic;
using System.Runtime.InteropServices;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

namespace AutoOrganize.Models.Args;

[StructLayout(LayoutKind.Auto)]
public readonly struct FileProcessArgs
{
    public MetadataType Type { get; init; }

    public required IEnumerable<string> FilesPaths { get; init; }

    public FileProcessArgs()
    {
    }
}