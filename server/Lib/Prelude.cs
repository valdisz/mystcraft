namespace advisor;

public static partial class Prelude {
    public static Unit Ignore<T>(this T self)
        => unit;
}
