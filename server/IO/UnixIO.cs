namespace advisor.IO;

using advisor.IO.Traits;

public readonly struct UnixIO : Traits.UnixIO {
    public readonly static Traits.UnixIO Default =
        new UnixIO();

    public Unit Chmod(string path, FilePermission permissions) {
        UnixInterop.Chmod(path, permissions);
        return unit;
    }
}
