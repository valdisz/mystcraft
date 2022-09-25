namespace advisor.Schema;

using HotChocolate.Types;

[InterfaceType("MutationResult")]
public interface IMutationResult {
    bool IsSuccess { get; }
    string Error { get; }
}

public abstract record MutationResult(bool IsSuccess, string Error = null) : IMutationResult {
    public static implicit operator bool(MutationResult result) => result.IsSuccess;
}
