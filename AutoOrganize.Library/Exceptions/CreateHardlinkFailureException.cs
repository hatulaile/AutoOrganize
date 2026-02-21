namespace AutoOrganize.Library.Exceptions;

public sealed class CreateHardlinkFailureException : Exception
{
    public CreateHardlinkFailureException(string message, int hResult) : base(message)
    {
        HResult = hResult;
    }
}