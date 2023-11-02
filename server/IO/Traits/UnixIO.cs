namespace advisor.IO.Traits;

using System;

[Flags]
public enum FilePermission {
    None = 0,

    UserRead = 0x100,
    UserWrite = 0x80,
    UserExecute = 0x40,
    UserAll = UserRead | UserWrite | UserExecute,

    GroupRead = 0x20,
    GroupWrite = 0x10,
    GroupExecute = 0x8,
    GroupAll = GroupRead | GroupWrite | GroupExecute,

    OtherRead = 0x4,
    OtherWrite = 0x2,
    OtherExecute = 0x1,
    OtherAll = OtherRead | OtherWrite | OtherExecute,

    All = UserAll | GroupAll | OtherAll
}

public interface UnixIO {
    Unit Chmod(string path, FilePermission permissions);
}
