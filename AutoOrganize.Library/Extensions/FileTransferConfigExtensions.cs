using AutoOrganize.Library.Services.FileTransferServices;

namespace AutoOrganize.Library.Extensions;

public static class FileTransferConfigExtensions
{
    extension(FileTransferConfig config)
    {
        public FileTransferOptions ToOption()
        {
            return new FileTransferOptions
            {
                CanOverwrite = config.CanOverwrite,
                Mode = config.Mode,
            };
        }
    }
}