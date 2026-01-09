namespace advisor.IO;

using System;
using System.Runtime.InteropServices;
using advisor.IO.Traits;

public static class UnixInterop {
    // user permissions
    public const int S_IRUSR = 0x100;
    public const int S_IWUSR = 0x80;
    public const int S_IXUSR = 0x40;

    // group permission
    public const int S_IRGRP = 0x20;
    public const int S_IWGRP = 0x10;
    public const int S_IXGRP = 0x8;

    // other permissions
    public const int S_IROTH = 0x4;
    public const int S_IWOTH = 0x2;
    public const int S_IXOTH = 0x1;

    [DllImport("libc", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern int chmod(string pathname, int mode);

    public static void Chmod(string path, FilePermission permissions) {
        var mode = 0;

        if (permissions.HasFlag(FilePermission.UserRead)) mode |= S_IRUSR;
        if (permissions.HasFlag(FilePermission.UserWrite)) mode |=S_IWUSR;
        if (permissions.HasFlag(FilePermission.UserExecute)) mode |= S_IXUSR;
        if (permissions.HasFlag(FilePermission.GroupRead)) mode |= S_IRGRP;
        if (permissions.HasFlag(FilePermission.GroupWrite)) mode |= S_IWGRP;
        if (permissions.HasFlag(FilePermission.GroupExecute)) mode |= S_IXGRP;
        if (permissions.HasFlag(FilePermission.OtherRead)) mode |= S_IROTH;
        if (permissions.HasFlag(FilePermission.OtherWrite)) mode |= S_IWOTH;
        if (permissions.HasFlag(FilePermission.OtherExecute)) mode |= S_IXOTH;

        var result = chmod(path, mode);
        if (result == -1) {
            var errno = Marshal.GetLastWin32Error();
            throw new Exception($"chmod failed with error {errno}");
        }
    }
}
