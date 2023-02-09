namespace advisor;

using System;

public static class Pipe {
    public static Func<A, C> PipeTo<A, B, C>(this Func<A, B> f, Func<B, C> g)
        => input => g(f(input));

    public static Func<B> PipeTo<A, B>(this Func<A> f, Func<A, B> g)
        => () => g(f());

    public static B PipeTo<A, B>(this A v, Func<A, B> f)
        => f(v);
}
