namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using advisor.Schema;

public record GameEngineDelete(long GameEngineId): IRequest<GameEngineDeleteResult>;

public record GameEngineDeleteResult(bool IsSuccess, string Error) : IMutationResult;

public class GameEngineDeleteHandler : IRequestHandler<GameEngineDelete, GameEngineDeleteResult> {
    public GameEngineDeleteHandler(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public async Task<GameEngineDeleteResult> Handle(GameEngineDelete request, CancellationToken cancellationToken) {
        var engine = await db.GameEngines.FindAsync(request.GameEngineId);

        if (engine == null) {
            return new GameEngineDeleteResult(false, "Game engine does not exist");
        }

        db.Remove(engine);
        await db.SaveChangesAsync();

        return new GameEngineDeleteResult(true, null);
    }
}
