namespace advisor.Model;
using LanguageExt.ClassInstances;

public sealed class FactionNumber : NumType<FactionNumber, TInt, int> {
    public FactionNumber(int value) : base(value) { }
}
