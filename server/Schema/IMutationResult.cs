namespace advisor
{
    using HotChocolate.Types;

    [InterfaceType("MutationResult")]
    public interface IMutationResult {
        bool IsSuccess { get; }
        string Error { get; }
    }
}
