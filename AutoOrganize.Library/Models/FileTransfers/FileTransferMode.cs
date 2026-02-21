namespace AutoOrganize.Library.Models.FileTransfers;

public enum FileTransferMode
{
    None = 0,
    HardLink = 1,
    SymbolicLink = 2,
    Copy = 3,
    Clipping = 4
}