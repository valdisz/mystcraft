namespace advisor.Features;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using advisor.Schema;
using advisor.Model;

public record GameEngineCreate(string Name, string Description, Stream Contents, Stream Ruleset): IRequest<GameEngineCreateResult>;

public record GameEngineCreateResult(bool IsSuccess, string Error = null, DbGameEngine Engine = null) : IMutationResult {
    public static GameEngineCreateResult New(Validation<Error, DbGameEngine> result) =>
        result.Match(
            Succ: ge => new GameEngineCreateResult(true, Engine: ge),
            Fail: e  => new GameEngineCreateResult(false, Error: e.Head.Message)
        );
}

public class GameEngineCreateHandler : IRequestHandler<GameEngineCreate, GameEngineCreateResult> {
    public GameEngineCreateHandler(Database database) {
        this.database = database;
    }

    private readonly Database database;

    public async Task<GameEngineCreateResult> Handle(GameEngineCreate request, CancellationToken cancellationToken) =>
        await (await Validate(request))
            .Map(GameInterpreter<Runtime>.Interpret)
            .Unwrap(Runtime.New(database, cancellationToken))
            .Map(GameEngineCreateResult.New);

    private static async Task<Validation<Error, Mystcraft<DbGameEngine>>> Validate(GameEngineCreate request) =>
    (
        ValidateName(request.Name).ForField("Name"),
        ValidateDescription(request.Description).ForField("Description"),
        NotEmpty(await request.Contents.ReadAllBytesAsync()).ForField(nameof(GameEngineCreate.Contents)),
        NotEmpty(await request.Ruleset.ReadAllBytesAsync()).ForField(nameof(GameEngineCreate.Ruleset))
    )
    .Apply(Mystcraft.CreateGameEngine);

    private static Validation<Error, string> ValidateName(string name) =>
        NotEmpty(name)
            .Bind(WithinLength(1, Some(Size.LABEL)));

    private static Validation<Error, string> ValidateDescription(string description) =>
        WithinLength(1, Some(Size.LABEL))(description);

}
