namespace advisor;

/// <summary>
/// Indicates value that is ment to be read-only.
/// </summary>
public readonly struct ReadOnly<A> {
    public ReadOnly(A Value) =>
        this.Value = Value;

    public readonly A Value;
}

public static class ReadOnly {
    public static ReadOnly<A> New<A>(A value) =>
        new ReadOnly<A>(value);
}
