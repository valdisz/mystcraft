namespace advisor.Features;

using MediatR;
using advisor.Persistence;
using advisor.Schema;
using advisor.Model;
using System.Threading;
using System.Threading.Tasks;

public record GameEngineDelete(long GameEngineId): IRequest<GameEngineDeleteResult>;

public record GameEngineDeleteResult(bool IsSuccess, string Error = null) : IMutationResult {
    public static GameEngineDeleteResult New(Validation<Error, LanguageExt.Unit> result) =>
        result.Match(
            Succ: ge => new GameEngineDeleteResult(true),
            Fail: e  => new GameEngineDeleteResult(false, Error: e.First().Message)
        );
}

public class GameEngineDeleteHandler : IRequestHandler<GameEngineDelete, GameEngineDeleteResult> {
    public GameEngineDeleteHandler(Database database) {
        this.database = database;
    }

    private readonly Database database;

    public Task<GameEngineDeleteResult> Handle(GameEngineDelete request, CancellationToken cancellationToken) =>
        Validate(request)
            .Map(GameInterpreter<Runtime>.Interpret)
            .RunWrapped(Runtime.New(database, cancellationToken))
            .Map(GameEngineDeleteResult.New);

    private static Validation<Error, Mystcraft<LanguageExt.Unit>> Validate(GameEngineDelete request) =>
        GameEngineId.New(request.GameEngineId)//.ForField(nameof(GameEngineDelete.GameEngineId))
            .Map(x =>
                from engine in Mystcraft.WriteOneGameEngine(x)
                from _ in Mystcraft.DeleteGameEngine(engine)
                select unit
            );

}
