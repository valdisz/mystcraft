namespace advisor;

using System.Threading.Tasks;

public static class AffExtensions {
    /// <summary>
    /// Returns the value of the effect or throws an exception if the effect failed.
    /// </summary>
    public static A Unwrap<A>(this Fin<A> fin) =>
        fin.Match(
            Succ: identity,
            Fail: ex => throw ex.ToException()
        );

    /// <summary>
    /// Returns the value of the effect or throws an exception if the effect failed.
    /// </summary>
    public static async ValueTask<A> Unwrap<A>(this ValueTask<Fin<A>> fin) =>
        (await fin).Match(
            Succ: identity,
            Fail: ex => throw ex.ToException()
        );
}
