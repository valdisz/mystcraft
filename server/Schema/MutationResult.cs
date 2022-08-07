namespace advisor
{
    public record MutationResult<T>(bool IsSuccess, T data, string Error) : IMutationResult;
}
