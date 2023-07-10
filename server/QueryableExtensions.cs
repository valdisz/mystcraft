namespace advisor;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Effects.Traits;
using Microsoft.EntityFrameworkCore;

public static class QueryableExtensions {
    /// <summary>
    /// Turns on or off entity change tracking for the given query.
    /// </summary>
    public static IQueryable<T> WithTracking<T>(this IQueryable<T> input, bool enabled) where T: class => enabled ? input : input.AsNoTracking();

    /// <summary>
    /// Returns the first element of a sequence as an Option.Some. If sequence is empty, returns Option.None.
    /// </summary>
    public static async ValueTask<Option<T>> HeadOrNoneAsync<T>(this IQueryable<T> input, CancellationToken ct = default) =>
        Optional(await input.Take(1).FirstOrDefaultAsync(ct));

    /// <summary>
    /// Returns the first element of a sequence as an Aff or fails with the provided error.
    /// </summary>
    public static Aff<RT, A> HeadOrFailAff<RT, A>(this IQueryable<A> input, Error error)
        where RT : struct, HasCancel<RT> =>
            AffMaybe<RT, A>(async rt => (await input.HeadOrNoneAsync(rt.CancellationToken))
                .Match(
                    Some: FinSucc,
                    None: () => FinFail<A>(error)
                )
            );

    /// <summary>
    /// Returns the first element of a sequence as an Aff or fails with the provided error. Error is constructed from the provided function on demand.
    /// </summary>
    public static Aff<RT, A> HeadOrFailAff<RT, A>(this IQueryable<A> input, Func<Error> error)
        where RT : struct, HasCancel<RT> =>
            AffMaybe<RT, A>(async rt => (await input.HeadOrNoneAsync(rt.CancellationToken))
                .Match(
                    Some: FinSucc,
                    None: () => FinFail<A>(error())
                )
            );
}
