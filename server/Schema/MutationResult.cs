namespace advisor.Schema;

using HotChocolate.Types;

[InterfaceType("MutationResult")]
public interface IMutationResult {
    bool IsSuccess { get; }
    string Error { get; }
}

public record MutationResult<T>(bool IsSuccess, T data, string Error) : IMutationResult;
