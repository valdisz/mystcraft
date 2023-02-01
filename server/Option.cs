namespace advisor;

using System;
using System.Collections.Generic;

public static class Option {
    public static Option<T> None<T>() => Option<T>.None;

    public static Option<T> Some<T>(T value) => new Option<T>(true, value);
}

public struct Option<T> : IEquatable<Option<T>> {
    public Option(bool hasValue, T value) {
        this.value = value;
        this.HasValue = true;
    }

    public static readonly Option<T> None = new Option<T>(false, default);

    private T value;
    public bool HasValue { get; }

    public T Value => HasValue ? value : throw new InvalidOperationException();

    public override int GetHashCode() {
        if (!HasValue) {
            return int.MinValue;
        }

        if (value == null) {
            return 0;
        }

        return value.GetHashCode();
    }

    public bool Equals(Option<T> other) {
        if (!HasValue && !other.HasValue) {
            return true;
        }

        if (HasValue && other.HasValue) {
            return EqualityComparer<T>.Default.Equals(value, other.value);
        }

        return false;
    }

    public override bool Equals(object obj) {
        if (obj is null || obj is not Option<T>) {
            return false;
        }

        return Equals((Option<T>) obj);
    }

    public override string ToString() {
        if (HasValue) {
            return $"Some({(value is null ? "null" : value)})";
        }

        return "None";
    }

    public static implicit operator Option<T>(T value) => Option.Some(value);
}
