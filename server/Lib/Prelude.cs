namespace advisor;

using System;

public static partial class Prelude {
    public static Unit Ignore<T>(this T self)
        => unit;

    public static AsyncIO<Unit> Ignore<T>(this AsyncIO<T> self)
        => self.Return(unit);

    public static T identity<T>(T self) => self;

    public static T invalidOp<T>() => throw new InvalidOperationException();
}
