namespace advisor
{
    using System;

    public class Maybe<T> {
        public Maybe(T value) {
            Success = true;
            this.value = value;
        }

        public Maybe(string error, int ln, int col) {
            Success = false;
            Error = error;
            Ln = ln;
            Col = col;
        }

        public static readonly Maybe<T> NA = new Maybe<T>("N/A", 0, 0);

        private T value;
        public T Value {
            get {
                if (!Success) throw new InvalidOperationException($"[{Ln}:{Col}] {Error}");
                return value;
            }
        }

        public bool Success { get; }
        public string Error { get; }
        public int Ln { get; }
        public int Col { get; }

        public override string ToString() => Success
            ? Value.ToString()
            : $"[ERROR] ({Ln}:{Col}) {Error}";

        public static implicit operator T(Maybe<T> v) => v.Value;

        public static implicit operator bool(Maybe<T> v) => v.Success;

        public Maybe<TOut> Convert<TOut>() {
            if (!Success) return new Maybe<TOut>(Error, Ln, Col);

            throw new InvalidCastException();
        }
    }
}
