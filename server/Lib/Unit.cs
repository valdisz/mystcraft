namespace advisor;

using System;
using System.Threading.Tasks;

public static partial class Prelude {
    public static Unit unit => Unit.Value;
}

public sealed class Unit : IEquatable<Unit> {
    private Unit() {

    }

    public static readonly Unit Value = new Unit();

    public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);

    public override int GetHashCode() => 0;

    public bool Equals(Unit other) => other is not null;

    public override bool Equals(object obj) => Equals(obj as Unit);

    public static bool operator ==(Unit a, Unit b) => a?.Equals(b) ?? false;

    public static bool operator !=(Unit a, Unit b) => !(a?.Equals(b) ?? false);

    public static bool operator ==(Unit a, object b) => a?.Equals(b) ?? false;

    public static bool operator !=(Unit a, object b) => !(a?.Equals(b) ?? false);
}
