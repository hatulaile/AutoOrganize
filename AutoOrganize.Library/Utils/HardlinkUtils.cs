using System.Runtime.InteropServices;
using AutoOrganize.Library.Exceptions;

namespace AutoOrganize.Library.Utils;

public static partial class HardlinkUtils
{
    public static void CreateHardlink(string path, string targetPath)
    {
        //todo: linux
        if (!WinCreateHardlinkInternal(path, targetPath, IntPtr.Zero))
        {
            int errorCode = Marshal.GetLastPInvokeError();
            throw new CreateHardlinkFailureException(Marshal.GetPInvokeErrorMessage(errorCode), errorCode);
        }
    }

    [LibraryImport("Kernel32.dll", SetLastError = true, EntryPoint = "CreateHardLinkW",
        StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool WinCreateHardlinkInternal(string path, string targetPath, IntPtr flags);
}