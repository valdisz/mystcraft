namespace advisor.Features;

using MediatR;
using advisor.Schema;
using advisor.Persistence;
using System.Threading.Tasks;
using System.Threading;
using advisor.Model;

public record GamePause(long GameId): IRequest<GamePauseResult>;

public record GamePauseResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error) {
    public static GamePauseResult New(Validation<Error, DbGame> result) =>
        result.Match(
            Succ: g => new GamePauseResult(true, Game: g),
            Fail: e => new GamePauseResult(false, Error: e.Head.Message)
        );
}

public class GamePauseHandler : IRequestHandler<GamePause, GamePauseResult> {
    public GamePauseHandler(Database database) {
        this.database = database;
    }

    private readonly Database database;

    public Task<GamePauseResult> Handle(GamePause request, CancellationToken cancellationToken) =>
        Validate(request)
            .Map(gameId => GameInterpreter<Runtime>.Interpret(
                from game in Mystcraft.WriteOneGame(gameId)
                from res in Mystcraft.Pause(game)
                select res
            ))
            .Unwrap(Runtime.New(database, cancellationToken))
            .Map(GamePauseResult.New);

    private static Validation<Error, GameId> Validate(GamePause request) =>
        GameId.New(request.GameId).ForField("GameId");
}
