namespace advisor.Schema;

using System.Threading.Tasks;
using HotChocolate.Types;

public interface IMutationResult {
    bool IsSuccess { get; }
    string Error { get; }
}

public class MutationResultType : InterfaceType<IMutationResult> {
    protected override void Configure(IInterfaceTypeDescriptor<IMutationResult> descriptor) {
        descriptor.Name("MutationResult");
    }
}

public abstract record MutationResult(bool IsSuccess, string Error = null) : IMutationResult {
    public static implicit operator bool(MutationResult result) => result.IsSuccess;
}
