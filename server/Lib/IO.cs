namespace advisor;

using System;

public delegate Result<T> IO<T>();

public static class IOExtensions {
    public static Result<T> Run<T>(this IO<T> self) {
        try {
            return self();
        }
        catch (Exception ex) {
            return Failure<T>(ex.Message, ex);
        }
    }

    public static IO<T> Flatten<T>(this IO<IO<T>> self)
        => () => self() switch {
            Result<IO<T>>.Success(var success) => success(),
            Result<IO<T>>.Failure(var failure) => Failure<T>(failure),
            _ => throw new InvalidOperationException()
        };

    public static IO<B> Select<A, B>(this IO<A> self, Func<A, B> selector)
        => () => self().Select(selector);

    public static IO<B> Select<A, B>(this IO<A> self, Func<A, Result<B>> selector)
        => () => self().Bind(selector);

    public static IO<B> Bind<A, B>(this IO<A> self, Func<A, IO<B>> selector)
        => self.Select(selector).Flatten();


    public static IO<T> OnFailure<T, R>(this IO<T> self, Func<Error, IO<R>> action)
        => () => self() switch {
            Result<T>.Success success => success,
            Result<T>.Failure failure => action(failure.Error) is Result<R>.Failure(var error)
                    ? Failure<T>(error)
                    : failure,
            _ =>throw new InvalidOperationException()
        };
}
