using System.Runtime.InteropServices;
using AutoOrganize.Library.Models.FileTransfers;

namespace AutoOrganize.Library.Services.FileTransferServices;

[StructLayout(LayoutKind.Auto)]
public struct FileTransferOptions
{
    public FileTransferMode Mode { get; set; }

    public bool CanOverwrite { get; set; }
}