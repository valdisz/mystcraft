namespace advisor;

using System;
using System.Threading.Tasks;

public delegate Result<T> IO<T>();

public delegate Task<Result<T>> AsyncIO<T>();

public static class IOExtensions {
    public static Result<T> Unwrap<T>(this IO<T> self) {
        try {
            return self();
        }
        catch (Exception ex) {
            return Failure<T>(ex.Message, ex);
        }
    }
    public static async Task<Result<T>> Run<T>(this AsyncIO<T> self) {
        try {
            return await self();
        }
        catch (Exception ex) {
            return Failure<T>(ex.Message, ex);
        }
    }

    public static IO<A> Flatten<A>(this IO<IO<A>> self)
        => () => self() switch {
            Result<IO<A>>.Success(var success) => success(),
            Result<IO<A>>.Failure(var failure) => Failure<A>(failure),
            _ => throw new InvalidOperationException()
        };

    public static AsyncIO<A> Flatten<A>(this AsyncIO<AsyncIO<A>> self)
        => async () => await self() switch {
            Result<AsyncIO<A>>.Success(var success) => await success(),
            Result<AsyncIO<A>>.Failure(var failure) => Failure<A>(failure),
            _ => throw new InvalidOperationException()
        };

    public static AsyncIO<A> Flatten<A>(this IO<AsyncIO<A>> self)
        => async () => self() switch {
            Result<AsyncIO<A>>.Success(var success) => await success(),
            Result<AsyncIO<A>>.Failure(var failure) => Failure<A>(failure),
            _ => throw new InvalidOperationException()
        };

    public static AsyncIO<A> Flatten<A>(this AsyncIO<IO<A>> self)
        => async () => await self() switch {
            Result<IO<A>>.Success(var success) => success(),
            Result<IO<A>>.Failure(var failure) => Failure<A>(failure),
            _ => throw new InvalidOperationException()
        };


    public static IO<B> Select<A, B>(this IO<A> self, Func<A, B> selector)
        => () => self().Select(selector);

    public static AsyncIO<B> Select<A, B>(this IO<A> self, Func<A, Task<B>> selector)
        => () => self().Select(selector);

    public static AsyncIO<B> Select<A, B>(this AsyncIO<A> self, Func<A, B> selector)
        => () => self().Select(selector);

    public static AsyncIO<B> Select<A, B>(this AsyncIO<A> self, Func<A, Task<B>> selector)
        => () => self().Select(selector);


    public static IO<B> Bind<A, B>(this IO<A> self, Func<A, IO<B>> selector)
        => self.Select(selector).Flatten();

    public static AsyncIO<B> Bind<A, B>(this IO<A> self, Func<A, AsyncIO<B>> selector)
        => self.Select(selector).Flatten();

    public static AsyncIO<B> Bind<A, B>(this AsyncIO<A> self, Func<A, IO<B>> selector)
        => self.Select(selector).Flatten();

    public static AsyncIO<B> Bind<A, B>(this AsyncIO<A> self, Func<A, AsyncIO<B>> selector)
        => self.Select(selector).Flatten();
}
