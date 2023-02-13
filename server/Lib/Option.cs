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

    public static T Unwrap<T>(this Option<T> self, T defaultValue)
        => self.HasValue ? (self as Option<T>.Some).Value : defaultValue;

    public static T Unwrap<T>(this Option<T> self, Func<T> defaultValue)
        => self.HasValue ? (self as Option<T>.Some).Value : defaultValue();

    public static Option<T> Flatten<T>(this Option<Option<T>> self)
        => self.Unwrap(None<T>());

    /// Alias of `Flatten`
    public static Option<T> Join<T>(this Option<Option<T>> self)
        => self.Flatten();


    public static Option<R> Select<T, R>(this Option<T> self, Func<T, R> selector)
        => self.HasValue ? selector(self.Unwrap()) : None<R>();


    public static Option<R> Bind<T, R>(this Option<T> self, Func<T, Option<R>> selector)
        => self.Select(selector).Flatten();


    /// Alias of `Bind`
    public static Option<R> SelectMany<T, R>(this Option<T> self, Func<T, Option<R>> selector)
        => self.Bind(selector);

    public static Option<R> SelectMany<T, U, R>(this Option<T> self, Func<T, Option<U>> k, Func<T, U, R> selector)
        => self.Bind(x => k(x).Select(y => selector(x, y)));


    public static Option<T> Do<T>(this Option<T> self, Action<T> action)
        => self.Select(x => { action(x); return x; });

    public static Option<T> AsOption<T>(this T value)
        => value is not null ? value : None<T>();

    public static Option<T> AsOption<T>(this T? value) where T: struct
        => value.HasValue ? value.Value : None<T>();

    public static async Task<Option<T>> AsOption<T>(this Task<T> value)
        => (await value).AsOption<T>();

    public static async Task<Option<T>> AsOption<T>(this Task<T?> value) where T: struct
        => (await value).AsOption<T>();
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
