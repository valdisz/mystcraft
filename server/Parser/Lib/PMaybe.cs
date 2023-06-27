namespace advisor
{
    using System;

    public class PMaybe<T> {
        public PMaybe(T value) {
            Success = true;
            this.value = value;
        }

        public PMaybe(string error, int ln, int col) {
            Success = false;
            Error = error;
            Ln = ln;
            Col = col;
        }

        public static readonly PMaybe<T> NA = new PMaybe<T>("N/A", 0, 0);

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

        public static implicit operator T(PMaybe<T> v) => v.Value;

        public static implicit operator bool(PMaybe<T> v) => v != null && v.Success;

        public PMaybe<TOut> Convert<TOut>() {
            if (!Success) return new PMaybe<TOut>(Error, Ln, Col);

            throw new InvalidCastException();
        }
    }
}
