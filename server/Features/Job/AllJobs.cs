namespace advisor.Features;

using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using System;
using Microsoft.Extensions.Logging;
using Hangfire;

public class AllJobs {
    public AllJobs(IMediator mediator, IUnitOfWork unit, IServiceProvider services, IBackgroundJobClient jobs, ILogger<AllJobs> logger) {
        this.mediator = mediator;
        this.unit = unit;
        this.services = services;
        this.jobs = jobs;
        this.logger = logger;
    }

    private readonly IUnitOfWork unit;
    private readonly IMediator mediator;
    private readonly IServiceProvider services;
    private readonly IBackgroundJobClient jobs;
    private readonly ILogger logger;

    public Task ReconcileAsync() => mediator.Send(new Reconcile());

    public Task SyncFactionsAsync(long gameId) => mediator.Send(new GameSyncFactions(gameId));

    public async Task RunTurnAsync(long gameId, int? turnNumber, GameNextTurnForceInput force) {
        logger.LogInformation("Starting turn processing");

        var gamesRep = unit.Games;

        var game = await gamesRep.GetOneAsync(gameId);

        try {
            if (turnNumber == null) {
                logger.LogInformation("Running next turn");
                var result = await EnusreSuccessAsync(mediator.Send(new TurnRun(gameId)));
                turnNumber = result.Turn.Number;
            }
            else {
                var turnsRepo = await unit.TurnsAsync(gameId);
                var turn = await turnsRepo.GetOneAsync(turnNumber.Value);

                if (turn.State == TurnState.PENDING) {
                    logger.LogInformation("Running next turn");
                    var result = await EnusreSuccessAsync(mediator.Send(new TurnRun(gameId)));
                    turnNumber = result.Turn.Number;
                }
            }

            await unit.BeginTransaction();

            logger.LogInformation("Parsing reports");
            await EnusreSuccessAsync(mediator.Send(new TurnParse(game, turnNumber.Value, Force: force?.Parse ?? false)));

            logger.LogInformation("Merging report state with game state");
            await EnusreSuccessAsync(mediator.Send(new TurnMerge(game, turnNumber.Value, Force: force?.Merge ?? false)));

            logger.LogInformation("Calculating statistics");
            await EnusreSuccessAsync(mediator.Send(new TurnProcess(game, turnNumber.Value, Force: force?.Process ?? false)));

            logger.LogInformation("Unlocking game");
            await gamesRep.UnlockAsync(gameId);

            await unit.CommitTransaction();

            logger.LogInformation($"Turn {turnNumber} is ready");
        }
        catch (Exception ex) {
            logger.LogError(ex, ex.Message);

            await unit.RollbackTransaction();

            throw;
        }
    }

    private async Task<T> EnusreSuccessAsync<T>(Task<T> resultTask) where T: IMutationResult {
        var result = await resultTask;
        if (result.IsSuccess) {
            return result;
        }

        throw new GameNextTurnException(result.Error);
    }
}

