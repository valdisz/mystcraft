namespace advisor;

using System;
using System.Threading.Tasks;

public delegate Task<Result<T>> AsyncIO<T>();

public static partial class Prelude {
    public static AsyncIO<T> AsyncEffect<T>(Func<Task<Result<T>>> action)
        => () => action();

    public static AsyncIO<T> AsyncEffect<T>(Func<Task<T>> action)
        => async () => Success(await action());

    public static AsyncIO<Unit> AsyncEffect(Func<Task> action) => async () => {
        await action();
        return Success(unit);
    };

    public static AsyncIO<T> AsyncEffect<T>(Func<Result<T>> action)
        => () => Task.FromResult(action());

    public static AsyncIO<T> AsyncEffect<T>(Func<T> action)
        => () => Task.FromResult(Success(action()));

    public static AsyncIO<T> AsyncEffect<T>(T value)
        => () => Task.FromResult(Success(value));
}

public static class AsyncIOExtensions {
    public static async Task<Result<T>> Run<T>(this AsyncIO<T> self) {
        try {
            return await self();
        }
        catch (Exception ex) {
            return Failure<T>(ex);
        }
    }

    public static AsyncIO<T> Finally<T>(this AsyncIO<T> self, Action finalizer)
        => () => {
            try {
                return self();
            }
            finally {
                finalizer();
            }
        };

    public static AsyncIO<T> Finally<T>(this AsyncIO<T> self, Func<Task> finalizer)
        => async () => {
            try {
                return await self();
            }
            finally {
                await finalizer();
            }
        };

    public static AsyncIO<T> Finally<T>(this IO<T> self, Func<Task> finalizer)
        => async () => {
            try {
                return self();
            }
            finally {
                await finalizer();
            }
        };

    public static AsyncIO<T> Flatten<T>(this AsyncIO<AsyncIO<T>> self)
        => async () => await self() switch {
            Result<AsyncIO<T>>.Success(var success) => await success(),
            Result<AsyncIO<T>>.Failure(var failure) => Failure<T>(failure),
            _ => throw new InvalidOperationException()
        };

    public static AsyncIO<T> Flatten<T>(this AsyncIO<IO<T>> self)
        => async () => await self() switch {
            Result<IO<T>>.Success(var success) => success(),
            Result<IO<T>>.Failure(var failure) => Failure<T>(failure),
            _ => throw new InvalidOperationException()
        };

    public static AsyncIO<T> Flatten<T>(this IO<AsyncIO<T>> self)
        => async () => self() switch {
            Result<AsyncIO<T>>.Success(var success) => await success(),
            Result<AsyncIO<T>>.Failure(var failure) => Failure<T>(failure),
            _ => throw new InvalidOperationException()
        };

    // public static AsyncIO<A> Flatten<A>(this IO<AsyncIO<A>> self)
    //     => async () => self() switch {
    //         Result<AsyncIO<A>>.Success(var success) => await success(),
    //         Result<AsyncIO<A>>.Failure(var failure) => Failure<A>(failure),
    //         _ => throw new InvalidOperationException()
    //     };

    // public static AsyncIO<A> Flatten<A>(this AsyncIO<IO<A>> self)
    //     => async () => await self() switch {
    //         Result<IO<A>>.Success(var success) => success(),
    //         Result<IO<A>>.Failure(var failure) => Failure<A>(failure),
    //         _ => throw new InvalidOperationException()
    //     };

    public static AsyncIO<R> Select<T, R>(this AsyncIO<T> self, Func<T, R> selector)
        => async () => (await self()).Select(selector);

    public static AsyncIO<T> AsAsync<T>(this IO<T> self)
        => () => Task.FromResult(self());

    public static AsyncIO<R> Return<T, R>(this AsyncIO<T> self, Func<R> selector)
        => async () => (await self()).Select(_ => selector());

    public static AsyncIO<R> Return<T, R>(this AsyncIO<T> self, R value)
        => async () => (await self()).Select(_ => value);

    public static AsyncIO<R> Bind<T, R>(this AsyncIO<T> self, Func<T, Result<R>> selector)
        => async () => (await self()).Select(selector).Flatten();

    public static AsyncIO<R> Bind<T, R>(this AsyncIO<T> self, Func<T, AsyncIO<R>> selector)
        => self.Select(selector).Flatten();

    public static AsyncIO<R> Bind<T, R>(this AsyncIO<T> self, Func<T, IO<R>> selector)
        => self.Select(selector).Flatten();

    public static AsyncIO<R> Bind<T, R>(this IO<T> self, Func<T, AsyncIO<R>> selector)
        => self.Select(selector).Flatten();

    public static AsyncIO<R> Bind<T, R>(this AsyncIO<T> self, Func<AsyncIO<R>> selector)
        => self.Select(_ => selector()).Flatten();

    public static AsyncIO<R> Bind<T, R>(this AsyncIO<T> self, Func<IO<R>> selector)
        => self.Select(_ => selector()).Flatten();

    public static AsyncIO<R> Bind<T, R>(this IO<T> self, Func<AsyncIO<R>> selector)
        => self.Select(_ => selector()).Flatten();


    // public static AsyncIO<B> Select<A, B>(this AsyncIO<A> self, Func<A, Task<B>> selector)
    //     => () => self().Select(selector);

    // public static AsyncIO<B> Select<A, B>(this IO<A> self, Func<A, Task<Result<B>>> selector)
    //     => () => self().Select(selector).Flatten();

    // public static AsyncIO<B> Select<A, B>(this AsyncIO<A> self, Func<A, Task<Result<B>>> selector)
    //     => () => self().Select(selector).Flatten();

    // public static AsyncIO<B> Bind<A, B>(this AsyncIO<A> self, Func<A, IO<B>> selector)
    //     => self.Select(selector).Flatten();

    // public static AsyncIO<R> Bind<A, B>(this AsyncIO<A> self, Func<A, AsyncIO<R>> selector)
    //     => self.Select(selector).Flatten();


    public static AsyncIO<T> OnFailure<T, R>(this AsyncIO<T> self, Func<Error, AsyncIO<R>> action)
        => async () => await self() switch {
            Result<T>.Success success => success,
            Result<T>.Failure failure => (await action(failure.Error)()) is Result<R>.Failure(var error)
                    ? Failure<T>(error)
                    : failure,
            _ =>throw new InvalidOperationException()
        };

    public static AsyncIO<T> OnFailure<T, R>(this AsyncIO<T> self, Func<AsyncIO<R>> action)
        => async () => await self() switch {
            Result<T>.Success success => success,
            Result<T>.Failure failure => (await action()()) is Result<R>.Failure(var error)
                    ? Failure<T>(error)
                    : failure,
            _ =>throw new InvalidOperationException()
        };

    public static AsyncIO<T> OnFailure<T, R>(this AsyncIO<T> self, Func<Error, IO<R>> action)
        => async () => await self() switch {
            Result<T>.Success success => success,
            Result<T>.Failure failure => action(failure.Error)() is Result<R>.Failure(var error)
                    ? Failure<T>(error)
                    : failure,
            _ =>throw new InvalidOperationException()
        };

    public static AsyncIO<T> Do<T>(this AsyncIO<T> self, Action<T> action)
        => async () => {
            var result = await self();
            if (result is Result<T>.Success success) {
                action(success.Value);
            }

            return result;
        };

    public static AsyncIO<T> Do<T>(this AsyncIO<T> self, Action action)
        => async () => {
            var result = await self();
            if (result.IsSuccess) {
                action();
            }

            return result;
        };

    public static AsyncIO<T> Do<T>(this AsyncIO<T> self, Func<T, Task> action)
        => async () => {
            var result = await self();
            if (result is Result<T>.Success success) {
                await action(success.Value);
            }

            return result;
        };

    public static AsyncIO<T> Do<T>(this AsyncIO<T> self, Func<Task> action)
        => async () => {
            var result = await self();
            if (result.IsSuccess) {
                await action();
            }

            return result;
        };
}
