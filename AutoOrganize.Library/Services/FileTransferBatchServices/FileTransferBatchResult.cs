namespace AutoOrganize.Library.Services.FileTransferBatchServices;

public class FileTransferBatchResult : IFileTransferBatchInfo
{
    public int Succeed { get; internal set; }

    public int Failed { get; internal set; }

    public int Total => Succeed + Failed;
}