namespace advisor.Model;

using LanguageExt.ClassInstances;

public sealed class TurnNumber : NumType<TurnNumber, TLong, long> {
    public TurnNumber(long value) : base(value) { }
}
