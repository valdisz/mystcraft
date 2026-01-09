namespace advisor.IO;

using advisor.IO.Traits;

public static class Unix<RT>
    where RT : struct, HasUnix<RT> {

    public static Eff<RT, Unit> Chmod(string path, FilePermission permissions) =>
        default(RT).UnixEff.Map(unix => unix.Chmod(path, permissions));
}
