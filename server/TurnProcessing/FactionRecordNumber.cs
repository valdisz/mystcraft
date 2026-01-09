namespace advisor.TurnProcessing;

using System;

public record struct FactionRecordNumber {
    private FactionRecordNumber(int num, bool isNew) {
        Num = num;
        IsNew = isNew;
    }

    public static readonly FactionRecordNumber New = new FactionRecordNumber(0, true);

    public static FactionRecordNumber Number(int number) => new FactionRecordNumber(number, false);

    public int Num { get; }
    public bool IsNew { get; }

    public T Match<T>(Func<T> New, Func<int, T> Number) => IsNew
        ? New()
        : Number(Num);

    override public string ToString() => IsNew
        ? "new"
        : Num.ToString();
}
