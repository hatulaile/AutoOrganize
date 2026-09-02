namespace AutoOrganize.Models.Args;

public readonly record struct RemoveTransferConfirmArgs(int SuccessCount, int FailedCount);

public readonly record struct RemoveTransferConfirmResult(bool IsConfirmed, bool IsDeleteFilesOnDisk);