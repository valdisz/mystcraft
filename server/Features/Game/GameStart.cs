namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using advisor.Model;

public record GameStart(long GameId): IRequest<GameStartResult>;

public record GameStartResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error) {
    public static GameStartResult New(Validation<Error, DbGame> result) =>
        result.Match(
            Succ: g => new GameStartResult(true, Game: g),
            Fail: e => new GameStartResult(false, Error: e.Head.Message)
        );
}

public class GameStartHandler : IRequestHandler<GameStart, GameStartResult> {
    public GameStartHandler(Database database) {
        this.database = database;
    }

    private readonly Database database;

    public Task<GameStartResult> Handle(GameStart request, CancellationToken cancellationToken) =>
        Validate(request)
            .Map(gameId => GameInterpreter<Runtime>.Interpret(
                from game in Mystcraft.WriteOneGame(gameId)
                from res in Mystcraft.Start(game)
                select res
            ))
            .Unwrap(Runtime.New(database, cancellationToken))
            .Map(GameStartResult.New);

    private static Validation<Error, GameId> Validate(GameStart request) =>
        GameId.New(request.GameId).ForField("GameId");
}
