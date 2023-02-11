namespace advisor;

using System;
using System.Threading.Tasks;

public static partial class Prelude {
    public static Result<T> Success<T>(T value) => new Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => new Result<T>.Failure(error);
    public static Result<T> Failure<T>(string message, Exception exception = null) => new Result<T>.Failure(new Error(message, exception));
}

public static class ResultExtensions {
    public static T Unwrap<T>(this Result<T> self) => self switch {
        Result<T>.Success some => some.Value,
        _ => throw new InvalidOperationException()
    };

    public static T Unwrap<T>(this Result<T> self, T defaultValue)
        => self.IsSuccess ? (self as Result<T>.Success).Value : defaultValue;

    public static T Unwrap<T>(this Result<T> self, Func<Error, T> defaultValue)
        => self switch {
            Result<T>.Success(var value) => value,
            Result<T>.Failure(var error) => defaultValue(error),
            _ => throw new InvalidOperationException()
        };


    public static async Task<T> Unwrap<T>(this Task<Result<T>> self) => await self switch {
        Result<T>.Success some => some.Value,
        _ => throw new InvalidOperationException()
    };

    public static async Task<T> Unwrap<T>(this Task<Result<T>> self, T defaultValue)
        => await self switch {
            Result<T>.Success(var value) => value,
            _ => defaultValue
        };

    public static async Task<T> Unwrap<T>(this Task<Result<T>> self, Func<Error, T> defaultValue)
        => await self switch {
            Result<T>.Success(var value) => value,
            Result<T>.Failure(var error) => defaultValue(error),
            _ => throw new InvalidOperationException()
        };


    public static Result<T> Flatten<T>(this Result<Result<T>> self)
        => self switch {
            Result<Result<T>>.Success(var value) => value,
            Result<Result<T>>.Failure(var error) => Failure<T>(error),
            _ => throw new InvalidOperationException()
        };

    /// Alias of `Flatten`
    public static Result<T> Join<T>(this Result<Result<T>> self)
        => self.Flatten();


    public static Result<R> Select<T, R>(this Result<T> self, Func<T, R> selector)
        => self switch {
            Result<T>.Success(var value) => Success(selector(value)),
            Result<T>.Failure(var error) => Failure<R>(error),
            _ => throw new InvalidOperationException()
        };

    public static async Task<Result<R>> Select<T, R>(this Result<T> self, Func<T, Task<R>> selector)
        => self switch {
            Result<T>.Success(var value) => Success(await selector(value)),
            Result<T>.Failure(var error) => Failure<R>(error),
            _ => throw new InvalidOperationException()
        };

    public static Result<R> Bind<T, R>(this Result<T> self, Func<T, Result<R>> selector)
        => self.Select(selector).Flatten();

    public static async Task<Result<R>> Bind<T, R>(this Result<T> self, Func<T, Task<Result<R>>> selector)
        => (await self.Select(selector)).Flatten();


    /// Alias of `Bind`
    public static Result<R> SelectMany<T, R>(this Result<T> self, Func<T, Result<R>> selector)
        => self.Bind(selector);

    public static Result<R> SelectMany<T, U, R>(this Result<T> self, Func<T, Result<U>> k, Func<T, U, R> selector)
        => self.Bind(x => k(x).Select(y => selector(x, y)));


    public static Result<T> OnFailure<T, R>(this Result<T> self, Func<Error, Result<R>> action)
        => self switch {
            Result<T>.Success success => success,
            Result<T>.Failure failure => action(failure.Error) is Result<R>.Failure(var error)
                    ? Failure<T>(error)
                    : failure,
            _ =>throw new InvalidOperationException()
        };


    public static Result<T> Do<T>(this Result<T> self, Action<T> action)
        => self.Select(x => { action(x); return x; });
}

public abstract class Result<T> {
    public abstract bool IsSuccess { get; }
    public abstract bool IsFailure { get; }

    public sealed class Success : Result<T> {
        public Success(T value) {
            Value = value;
        }

        public T Value { get; }

        public override bool IsSuccess => true;

        public override bool IsFailure => false;

        public void Deconstruct(out T value) {
            value = Value;
        }
    }

    public sealed class Failure : Result<T> {
        public Failure(Error error) {
            Error = error;
        }

        public Error Error { get; }

        public override bool IsSuccess => false;

        public override bool IsFailure => true;

        public void Deconstruct(out Error error) {
            error = Error;
        }
    }
}
