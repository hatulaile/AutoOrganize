namespace AutoOrganize.Library.Services.FileTransferBatchServices;

public readonly struct FileTransferBatchResult : IFileTransferBatchInfo
{
    public int Succeed { get; }

    public int Failed { get; }

    public int Total => Succeed + Failed;

    public FileTransferBatchResult(int succeed, int failed)
    {
        Succeed = succeed;
        Failed = failed;
    }
}