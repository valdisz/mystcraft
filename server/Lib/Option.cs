namespace advisor;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static partial class Prelude {
    public static Option<T> None<T>() => Option<T>.None.Instance;

    public static Option<T> Some<T>(T value) => new Option<T>.Some(value);
}

public static class OptionExtensions {
    public static T Unwrap<T>(this Option<T> self) => self switch {
        Option<T>.Some some => some.Value,
        _ => throw new InvalidOperationException()
    };

    public static async Task<T> Unwrap<T>(this Task<Option<T>> self)
        => (await self).Unwrap();

    public static T Unwrap<T>(this Option<T> self, T defaultValue)
        => self.HasValue ? (self as Option<T>.Some).Value : defaultValue;

    public static T Unwrap<T>(this Option<T> self, Func<T> defaultValue)
        => self.HasValue ? (self as Option<T>.Some).Value : defaultValue();

    public static async Task<T> Unwrap<T>(this Task<Option<T>> self, Func<Task<T>> defaultValue) {
        var option = await self;

        return option.HasValue ? (self as Option<T>.Some).Value : await defaultValue();
    }

    public static async Task<T> Unwrap<T>(this Task<Option<T>> self, T defaultValue)
        => (await self).Unwrap(defaultValue);

    public static async Task<T> Unwrap<T>(this Task<Option<T>> self, Func<T> defaultValue)
        => (await self).Unwrap(defaultValue);

    public static Option<T> Flatten<T>(this Option<Option<T>> self)
        => self.Unwrap(None<T>());

    public static Task<Option<T>> Flatten<T>(this Task<Option<Option<T>>> self)
        => self.Unwrap(None<T>());

    /// Alias of `Flatten`
    public static Option<T> Join<T>(this Option<Option<T>> self)
        => self.Flatten();

    public static Task<Option<T>> Join<T>(this Task<Option<Option<T>>> self)
        => self.Flatten();


    public static Option<R> Select<T, R>(this Option<T> self, Func<T, R> selector)
        => self.HasValue ? selector(self.Unwrap()) : None<R>();

    public static async Task<Option<TResult>> Select<T, TResult>(this Task<Option<T>> self, Func<T, Task<TResult>> selector) {
        var option = await self;
        return option.HasValue ? await selector(option.Unwrap()) : None<TResult>();
    }

    public static async Task<Option<TResult>> Select<T, TResult>(this Task<Option<T>> self, Func<T, TResult> selector)
        => (await self).Select(selector);

    public static async Task<Option<R>> Select<T, R>(this Option<T> self, Func<T, Task<R>> selector)
        => self switch {
            Option<T>.Some(var value) => Some(await selector(value)),
            Option<T>.None => None<R>(),
            _ => throw new InvalidOperationException()
        };


    public static Option<R> Bind<T, R>(this Option<T> self, Func<T, Option<R>> selector)
        => self.Select(selector).Flatten();

    public static Task<Option<R>> Bind<T, R>(this Task<Option<T>> self, Func<T, Task<Option<R>>> selector)
        => self.Select(selector).Flatten();

    public static Task<Option<R>> Bind<T, R>(this Task<Option<T>> self, Func<T, Option<R>> selector)
        => self.Select(selector).Flatten();

    public static async Task<Option<R>> Bind<T, R>(this Option<T> self, Func<T, Task<Option<R>>> selector)
        => (await self.Select(selector)).Flatten();

    /// Alias of `Bind`
    public static Option<R> SelectMany<T, R>(this Option<T> self, Func<T, Option<R>> selector)
        => self.Bind(selector);

    public static Option<R> SelectMany<T, U, R>(this Option<T> self, Func<T, Option<U>> k, Func<T, U, R> selector)
        => self.Bind(x => k(x).Select(y => selector(x, y)));

    public static Option<T> AsOption<T>(this T value) => value is not null ? value : None<T>();

    public static async Task<Option<T>> AsOptionAsync<T>(this Task<T> value)
        => value.IsCompleted ? value.Result : await value;
}

public abstract class Option<T> : IEquatable<Option<T>>, IEquatable<T> {
    public abstract bool HasValue { get; }

    public sealed class None : Option<T>, IEquatable<None> {
        private None() { }

        public static readonly Option<T> Instance = new None();

        public override bool HasValue => false;

        public override int GetHashCode() => int.MinValue;

        public bool Equals(None other) => other is not null;

        public override bool Equals(Option<T> other) => Equals(other as None);

        public override bool Equals(T? other) => false;

        public override bool Equals(object obj) => Equals(obj as None);

        public override string ToString() => "None";
    }

    public sealed class Some : Option<T>, IEquatable<Some> {
        public Some(T value) {
            this.Value = value;
        }

        public T Value { get; }

        public override bool HasValue => true;

        public void Deconstruct(out T value) => value = Value;

        public override int GetHashCode() {
            if (Value is null) {
                return 0;
            }

            return Value.GetHashCode();
        }

        public bool Equals(Some other) {
            if (other is null) {
                return false;
            }

            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(Option<T> other) => Equals(other as Some);

        public override bool Equals(T? other) => Equals(Some(other));

        public override bool Equals(object obj) {
            switch (obj) {
                case Some value:
                    return Equals(value);

                case T exactValue:
                    return Equals(exactValue);

                default:
                    return false;
            }
        }

        public override string ToString() => $"Some({(Value is null ? "null" : Value)})";
    }

    public override int GetHashCode() => 0;

    public abstract bool Equals(Option<T> other);

    public abstract bool Equals(T? other);

    public override bool Equals(object obj) {
        switch (obj) {
            case Option<T> value:
                return Equals(value);

            case T exactValue:
                return Equals(exactValue);

            default:
                return false;
        }
    }

    public static implicit operator Option<T>(T? value) => value is null ? None<T>() : Some(value);

    public static explicit operator T(Option<T> source) => source.Unwrap();

    public static bool operator ==(Option<T> a, Option<T> b) => a.Equals(b);

    public static bool operator !=(Option<T> a, Option<T> b) => !a.Equals(b);

    public static bool operator ==(Option<T> a, T b) => a.Equals(b);

    public static bool operator !=(Option<T> a, T b) => !a.Equals(b);

    public static bool operator ==(Option<T> a, object b) => a.Equals(b);

    public static bool operator !=(Option<T> a, object b) => !a.Equals(b);
}
