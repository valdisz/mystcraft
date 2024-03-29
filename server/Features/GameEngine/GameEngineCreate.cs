namespace advisor.Features;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using advisor.Schema;

public record GameEngineCreate(string Name, Stream Contents): IRequest<GameEngineCreateResult>;

public record GameEngineCreateResult(bool IsSuccess, string Error = null, DbGameEngine Engine = null) : IMutationResult;

public class GameEngineCreateHandler : IRequestHandler<GameEngineCreate, GameEngineCreateResult> {
    public GameEngineCreateHandler(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public async Task<GameEngineCreateResult> Handle(GameEngineCreate request, CancellationToken cancellationToken) {
        using var buffer = new MemoryStream();
        await request.Contents.CopyToAsync(buffer);

        var ge = new DbGameEngine {
            Name = request.Name,
            Contents = buffer.ToArray(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await db.GameEngines.AddAsync(ge);

        await db.SaveChangesAsync();

        return new GameEngineCreateResult(true, Engine: ge);
    }
}
