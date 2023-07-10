namespace advisor;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Effects.Traits;
using Microsoft.EntityFrameworkCore;

public static class QueryableExtensions {
    public static IQueryable<T> WithTracking<T>(this IQueryable<T> input, bool enabled) where T: class => enabled ? input : input.AsNoTracking();

    public static async ValueTask<Option<T>> HeadOrNoneAsync<T>(this IQueryable<T> input, CancellationToken ct = default) =>
        Optional(await input.FirstOrDefaultAsync(ct));

    public static Aff<RT, A> HeadOrFailAff<RT, A>(this IQueryable<A> input, Error error)
        where RT : struct, HasCancel<RT> =>
            AffMaybe<RT, A>(async rt => (await input.HeadOrNoneAsync(rt.CancellationToken))
                .Match(
                    Some: FinSucc,
                    None: () => FinFail<A>(error)
                )
            );
}
