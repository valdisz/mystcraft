namespace advisor.Features;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using advisor.Schema;
using advisor.Model;

public record GameEngineCreateLocal(string Name, string Description, Stream Engine, Stream Ruleset): IRequest<GameEngineCreateLocalResult>;

public record GameEngineCreateLocalResult(bool IsSuccess, string Error = null, DbGameEngine Engine = null) : IMutationResult {
    public static GameEngineCreateLocalResult New(Validation<Error, DbGameEngine> result) =>
        result.Match(
            Succ: ge => new GameEngineCreateLocalResult(true, Engine: ge),
            Fail: e  => new GameEngineCreateLocalResult(false, Error: e.Head.Message)
        );
}

public class GameEngineCreateLocalHandler : IRequestHandler<GameEngineCreateLocal, GameEngineCreateLocalResult> {
    public GameEngineCreateLocalHandler(Database database) {
        this.database = database;
    }

    private readonly Database database;

    public async Task<GameEngineCreateLocalResult> Handle(GameEngineCreateLocal request, CancellationToken cancellationToken) =>
        await (await Validate(request))
            .Map(GameInterpreter<Runtime>.Interpret)
            .Unwrap(Runtime.New(database, cancellationToken))
            .Map(GameEngineCreateLocalResult.New);

    private static async Task<Validation<Error, Mystcraft<DbGameEngine>>> Validate(GameEngineCreateLocal request) =>
    (
        ValidateName(request.Name).ForField("Name"),
        ValidateDescription(request.Description).ForField("Description"),
        NotEmpty(await request.Engine.ReadAllBytesAsync()).ForField(nameof(GameEngineCreateLocal.Engine)),
        NotEmpty(await request.Ruleset.ReadAllBytesAsync()).ForField(nameof(GameEngineCreateLocal.Ruleset))
    )
    .Apply(Mystcraft.CreateGameEngine);

    private static Validation<Error, string> ValidateName(string name) =>
        NotEmpty(name)
            .Bind(WithinLength(1, Some(Size.LABEL)));

    private static Validation<Error, string> ValidateDescription(string description) =>
        WithinLength(1, Some(Size.LABEL))(description);

}
