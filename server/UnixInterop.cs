namespace advisor;

using System.Runtime.InteropServices;

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

    [DllImport("libc", SetLastError = true)]
    public static extern int chmod(string pathname, int mode);
}
