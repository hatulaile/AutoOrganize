namespace AutoOrganize.Models;

public readonly struct SelectSystemFilesModel
{
    public string Path { get; }

    public bool IsFile { get; }

    public SelectSystemFilesModel(string path, bool isFile)
    {
        Path = path;
        IsFile = isFile;
    }
}