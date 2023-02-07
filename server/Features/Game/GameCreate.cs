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

    public async Task<GameCreateLocalResult> Handle(GameCreateLocal request, CancellationToken cancellationToken) {
        var game = DbGame.CreateLocal(
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
        );

        return (await CreateGameAsync(game, cancellationToken)) switch {
            var (result, _) when result is not null => new GameCreateLocalResult(true, Game: result),
            var (_, error) => new GameCreateLocalResult(false, Error: error)
        };
    }

    public async Task<GameCreateRemoteResult> Handle(GameCreateRemote request, CancellationToken cancellationToken) {
        var game = DbGame.CreateRemote(
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
        );

        return (await CreateGameAsync(game, cancellationToken)) switch {
            var (result, _) when result is not null => new GameCreateRemoteResult(true, Game: result),
            var (_, error) => new GameCreateRemoteResult(false, Error: error)
        };
    }

    private async Task<(DbGame result, string error)> CreateGameAsync(DbGame game, CancellationToken cancellation) {
        var result = gameRepo.Add(game);

        await unitOfWork.BeginTransaction(cancellation);

        try {
            await unitOfWork.SaveChanges(cancellation);
        }
        catch (Exception ex) {
            logger.LogWarning(ex, "Cannot create game");
            await unitOfWork.RollbackTransaction(cancellation);

            return (null, "Cannot create game");
        }

        await mediator.Send(new Reconcile(result.Id), cancellation);

        if ((await unitOfWork.CommitTransaction(cancellation).Run()).IsFailure) {
            return (null, "Failed to commit transaction");
        }

        return (result, null);
    }
}
