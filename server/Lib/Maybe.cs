#nullable enable

namespace advisor;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public readonly struct Identity<T> {
    public Identity(T value) {
        this.Value = value;
    }

    public readonly T Value;

    public void Deconstruct(out T value) => value = Value;

    public Identity<R> Map<R>(Func<T, R> projection) => new Identity<R>(projection(Value));
    public Identity<R> Select<R>(Func<T, R> projection) => new Identity<R>(projection(Value));
    public Identity<B> Bind<B>(Func<T, Identity<B>> projection) => projection(Value);
    public Identity<B> SelectMany<B>(Func<T, Identity<B>> projection) => projection(Value);
    public Identity<C> SelectMany<B, C>(Func<T, Identity<B>> bind, Func<T, B, C> project) {
        var v = Value;
        return bind(v).Map(b => project(v, b));
    }
}

// using System.Threading.Tasks;

public readonly struct Option<T> : IEquatable<Option<T>>, IEquatable<T> {
    public Option() {
        this.HasValue = false;
        this.Value = default;
    }

    public Option([NotNull] T value) {
        if (value is null) throw new ArgumentNullException(nameof(value));

        this.Value = value;
        this.HasValue = true;
    }

    public static readonly Option<T> NoneInstance = new Option<T>();

    public bool HasValue { get; }

    private T Value { get; }

    public bool IsSome => HasValue;

    public bool IsNone => !HasValue;

    // public sealed class None : Option<T>, IEquatable<None> {
    //     private None() { }


    //     public override bool HasValue => false;

    //     public override int GetHashCode() => int.MinValue;

    //     public bool Equals(None? other) => other is not null;

    //     public override bool Equals(Option<T>? other) => Equals(other as None);

    //     public override bool Equals(T? other) => other is null;

    //     public override bool Equals(object? obj) => Equals(obj as None);

    //     public override string ToString() => "None";
    // }

    // public sealed class Some : Option<T>, IEquatable<Some> {
    //     public Some([NotNull] T value) {
    //         if (value is null) throw new ArgumentNullException(nameof(value));

    //         this.Value = value;
    //     }

    //     [NotNull]
    //     public T Value { get; }

    //     public override bool HasValue => true;

    //     public void Deconstruct(out T value) => value = Value;

    //     public override int GetHashCode() => Value.GetHashCode();

    //     public bool Equals(Some? other) => other is null
    //         ? false
    //         : EqualityComparer<T>.Default.Equals(Value, other.Value);

    //     public override bool Equals(Option<T>? other) => Equals(other as Some);

    //     public override bool Equals(T? other) => other is null
    //         ? false
    //         : Equals(Some(other));

    //     public override bool Equals(object? obj) {
    //         switch (obj) {
    //             case Some value:
    //                 return Equals(value);

    //             case T exactValue:
    //                 return Equals(exactValue);

    //             default:
    //                 return false;
    //         }
    //     }

    //     public override string ToString() => $"Some({Value})";
    // }

    public override int GetHashCode() => this switch {
        { IsNone: true } => 0,
        #nullable disable
        _ => Value.GetHashCode()
        #nullable enable
    };

    public bool Equals(Option<T>? other) => IsSome && (other?.IsSome ?? false) && (Value?.Equals(other.Value) ?? false);

    public bool Equals(T? other) => IsSome && (other?.Equals(Value) ?? false);

    public override bool Equals(object? obj) => obj switch {
        Option<T> opt => Equals(opt),
        T value => Equals(value),
        _ => false
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public R Match<R>(Func<T, R> some, Func<R> none) => HasValue
        ? (some is null ? throw new ArgumentNullException(nameof(some)) : some(Value))
        : (none is null ? throw new ArgumentNullException(nameof(none)) : none());


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap(Func<T> defaultValue) => Match(some: identity, none: defaultValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap() => Match(some: identity, none: invalidOp<T>);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap(T defaultValue) => Match(some: identity, none: () => defaultValue);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T value)
        => value = Unwrap();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<R> Map<R>(Func<T, R> projection)
        => IsSome ? Some(projection(Unwrap())) : None<R>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<R> Select<R>(Func<T, R> projection)
        => this.Map(projection);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<R> Bind<R>(Func<T, Option<R>> projection)
        => IsSome ? projection(Unwrap()) : None<R>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<R> SelectMany<R>(Func<T, Option<R>> projection)
        => this.Bind(projection);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<R> SelectMany<U, R>(Func<T, Option<U>> bind, Func<T, U, R> projection) =>
        this.Bind(t => bind(t).Map(u => projection(t, u)));


    public static implicit operator Option<T>(T? value) => value is null
        ? None<T>()
        : Some(value);

    public static explicit operator T(Option<T> source) => source.Unwrap();

    public static bool operator ==(Option<T>? a, Option<T>? b) => a?.Equals(b) ?? false;

    public static bool operator !=(Option<T>? a, Option<T>? b) => !(a?.Equals(b) ?? false);

    public static bool operator ==(Option<T>? a, T? b) => a?.Equals(b) ?? false;

    public static bool operator !=(Option<T>? a, T? b) => !(a?.Equals(b) ?? false);

    public static bool operator ==(Option<T>? a, object? b) => (a?.Equals(b) ?? false);

    public static bool operator !=(Option<T>? a, object? b) => !(a?.Equals(b) ?? false);
}

public static partial class Prelude {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> None<T>() => Option<T>.NoneInstance;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some<T>(T value) => new Option<T>(value);
}

public static class OptionExtensions {
    ///// Flatten
    public static Option<T> Flatten<T>(this Option<Option<T>> self)
        => self.Unwrap(None<T>());

    /// Alias of `Flatten`
    public static Option<T> Join<T>(this Option<Option<T>> self)
        => self.Flatten();



    ///// Map
    public static Option<R> Select<T, R>(this Option<T> self, Func<T, R> selector) => self switch {
        Option<T>.Some some => Some<R>(selector(some.Value)),
        _ => None<R>()
    };

    public static Option<R> Map<T, R>(this Option<T> self, Func<T, R> projection)
        => self.Select(projection);



    ///// Bind
    public static Option<R> Bind<T, R>(this Option<T> self, Func<T, Option<R>> selector)
        => self.Select(selector).Flatten();



    ///// LINQ
    public static Option<R> SelectMany<T, R>(this Option<T> self, Func<T, Option<R>> selector)
        => self.Bind(selector);

    public static Option<R> SelectMany<T, U, R>(this Option<T> self, Func<T, Option<U>> bind, Func<T, U, R> selector)
        => self.Bind(x => bind(x).Select(y => selector(x, y)));



    // public static Maybe<T> Do<T>(this Maybe<T> self, Action<T> action)
    //     => self.Select(x => { action(x); return x; });

    // public static Maybe<T> AsOption<T>(this T value)
    //     => value is not null ? value : None<T>();

    // public static Maybe<T> AsOption<T>(this T? value) where T: struct
    //     => value.HasValue ? value.Value : None<T>();

    // public static async Task<Maybe<T>> AsOption<T>(this Task<T> value)
    //     => (await value).AsOption<T>();

    // public static async Task<Maybe<T>> AsOption<T>(this Task<T?> value) where T: struct
    //     => (await value).AsOption<T>();
}
