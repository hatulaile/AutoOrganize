namespace AutoOrganize.Library.Exceptions;

public sealed class InvalidOutputPathException : Exception
{
    public string OutputPath { get; }

    public InvalidOutputPathException(string outputPath)
        : base($"Output path is invalid: '{outputPath}'.")
    {
        OutputPath = outputPath;
    }

    public InvalidOutputPathException(string outputPath, string message)
        : base(message)
    {
        OutputPath = outputPath;
    }
}