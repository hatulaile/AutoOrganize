using System.Runtime.InteropServices;
using AutoOrganize.Library.Models.FileTransfers;

namespace AutoOrganize.Library.Services.FileTransferServices;

[StructLayout(LayoutKind.Auto)]
public readonly struct FileTransferOptions
{
    public FileTransferOptions()
    {
    }

    public FileTransferMode Mode { get; init; }

    public bool CanOverwrite { get; init; }
}