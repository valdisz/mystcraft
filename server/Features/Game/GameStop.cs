namespace advisor.Features;

using MediatR;
using advisor.Schema;
using advisor.Persistence;
using System.Threading.Tasks;
using System.Threading;
using advisor.Model;

public record GameStop(long GameId): IRequest<GameStopResult>;

public record GameStopResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error) {
    public static GameStopResult New(Validation<Error, DbGame> result) =>
        result.Match(
            Succ: g => new GameStopResult(true, Game: g),
            Fail: e => new GameStopResult(false, Error: e.Head.Message)
        );
}

public class GameStopHandler : IRequestHandler<GameStop, GameStopResult> {
    public GameStopHandler(Database database) {
        this.database = database;
    }

    private readonly Database database;

    public Task<GameStopResult> Handle(GameStop request, CancellationToken cancellationToken) =>
        Validate(request)
            .Map(gameId => GameInterpreter<Runtime>.Interpret(
                from game in Mystcraft.WriteOneGame(gameId)
                from res in Mystcraft.Stop(game)
                select res
            ))
            .RunWrapped(Runtime.New(database, cancellationToken))
            .Map(GameStopResult.New);

    private static Validation<Error, GameId> Validate(GameStop request) =>
        GameId.New(request.GameId).ForField("GameId");
}
