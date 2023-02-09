namespace advisor.Features;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;
using Microsoft.Extensions.Logging;

public record GameCreateLocal(
    string Name,
    long EngineId,
    Stream Ruleset,
    List<MapLevel> Map,
    string Schedule,
    string TimeZone,
    DateTimeOffset? StartAt,
    DateTimeOffset? FinishAt
) : IRequest<GameCreateLocalResult>;

public record GameCreateLocalResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public record GameCreateRemote(
    string Name,
    string ServerAddress,
    Stream Ruleset,
    List<MapLevel> Map,
    string Schedule,
    string TimeZone,
    DateTimeOffset? StartAt,
    DateTimeOffset? FinishAt
) : IRequest<GameCreateRemoteResult>;
public record GameCreateRemoteResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GameCreateHandler :
    IRequestHandler<GameCreateLocal, GameCreateLocalResult>,
    IRequestHandler<GameCreateRemote, GameCreateRemoteResult> {

    public GameCreateHandler(IGameRepository gameRepo, IMediator mediator, ILogger<GameCreateHandler> logger) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.mediator = mediator;
        this.logger = logger;
    }

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMediator mediator;
    private readonly ILogger logger;

    public async Task<GameCreateLocalResult> Handle(GameCreateLocal request, CancellationToken cancellationToken)
        => await CreateGameAsync(
            DbGame.CreateLocal(
                name: request.Name,
                engineId: request.EngineId,
                ruleset: await request.Ruleset.ReadAllBytesAsync(cancellationToken),
                options: new GameOptions {
                    Map = request.Map,
                    Schedule = request.Schedule,
                    TimeZone = request.TimeZone,
                    StartAt = request.StartAt,
                    FinishAt = request.FinishAt
                }
            ),
            x => new GameCreateLocalResult(true, Game: x),
            error => new GameCreateLocalResult(false, Error: error.Message),
        cancellationToken);

    public async Task<GameCreateRemoteResult> Handle(GameCreateRemote request, CancellationToken cancellationToken)
        => await CreateGameAsync(
            DbGame.CreateRemote(
                name: request.Name,
                serverAddress: request.ServerAddress,
                ruleset: await request.Ruleset.ReadAllBytesAsync(cancellationToken),
                options: new GameOptions {
                    Map = request.Map,
                    Schedule = request.Schedule,
                    TimeZone = request.TimeZone,
                    StartAt = request.StartAt,
                    FinishAt = request.FinishAt
                }
            ),
            x => new GameCreateRemoteResult(true, Game: x),
            error => new GameCreateRemoteResult(false, Error: error.Message),
        cancellationToken);

    private Task<T> CreateGameAsync<T>(DbGame game, Func<DbGame, T> onSuccess, Func<Error, T> onFailure, CancellationToken cancellation)
        => unitOfWork.BeginTransaction(cancellation)
            .Select(_ => gameRepo.Add(game))
            .Bind(g => unitOfWork.SaveChanges(cancellation)
                .Bind(() => GameFunctions.Reconcile(g.Id, mediator, cancellation))
                .Bind(() => unitOfWork.CommitTransaction(cancellation))
                .Return(g)
            )
            .PipeTo(unitOfWork.RunWithRollback<DbGame, T>(onSuccess, onFailure, cancellation));
}
