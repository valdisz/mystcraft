namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using advisor.Schema;
using advisor.Model;

public record GameEngineCreateRemote(string Name, string Description, string Api, string Options): IRequest<GameEngineCreateRemoteResult>;

public record GameEngineCreateRemoteResult(bool IsSuccess, string Error = null, DbGameEngine Engine = null) : IMutationResult {
    public static GameEngineCreateRemoteResult New(Validation<Error, DbGameEngine> result) =>
        result.Match(
            Succ: ge => new GameEngineCreateRemoteResult(true, Engine: ge),
            Fail: e  => new GameEngineCreateRemoteResult(false, Error: e.Head.Message)
        );
}

public class GameEngineCreateRemoteHandler : IRequestHandler<GameEngineCreateRemote, GameEngineCreateRemoteResult> {
    public GameEngineCreateRemoteHandler(Database database) {
        this.database = database;
    }

    private readonly Database database;

    public Task<GameEngineCreateRemoteResult> Handle(GameEngineCreateRemote request, CancellationToken cancellationToken) =>
        Validate(request)
            .Map(GameInterpreter<Runtime>.Interpret)
            .Unwrap(Runtime.New(database, cancellationToken))
            .Map(GameEngineCreateRemoteResult.New);

    private static Validation<Error, Mystcraft<DbGameEngine>> Validate(GameEngineCreateRemote request) =>
    (
        ValidateName(request.Name).ForField(nameof(GameEngineCreateRemote.Name)),
        ValidateDescription(request.Description).ForField(nameof(GameEngineCreateRemote.Description)),
        ValidateApi(request.Api).ForField(nameof(GameEngineCreateRemote.Api)),
        ValidateOptions(request.Options).ForField(nameof(GameEngineCreateRemote.Options))
    )
    .Apply(Mystcraft.CreateGameEngineRemote);

    private static Validation<Error, string> ValidateName(string name) =>
        NotEmpty(name)
            .Bind(WithinLength(1, Some(Size.LABEL)));

    private static Validation<Error, string> ValidateDescription(string description) =>
        WithinLength(0, Some(Size.LABEL))(description);

    private static Validation<Error, string> ValidateApi(string api) =>
        NotEmpty(api)
            .Bind(WithinLength(1, Some(Size.SYMBOL)));

    private static Validation<Error, string> ValidateOptions(string options) =>
        WithinLength(0, Some(Size.MAX_TEXT))(options);
}
