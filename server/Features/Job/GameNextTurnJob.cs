namespace advisor.Features;

using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using System;
using Microsoft.Extensions.Logging;

public class GameNextTurnJob {
    public GameNextTurnJob(IUnitOfWork unit, IMediator mediator, IServiceProvider services, ILogger<GameNextTurnJob> logger) {
        this.unit = unit;
        this.mediator = mediator;
        this.services = services;
        this.logger = logger;
    }

    private readonly IUnitOfWork unit;
    private readonly IMediator mediator;
    private readonly IServiceProvider services;
    private readonly ILogger<GameNextTurnJob> logger;

    public async Task RunAsync(long gameId, int? turnNumber) {
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

            await unit.BeginTransactionAsync();

            logger.LogInformation("Parsing reports");
            await EnusreSuccessAsync(mediator.Send(new TurnParse(game, turnNumber.Value)));

            logger.LogInformation("Merging report state with game state");
            await EnusreSuccessAsync(mediator.Send(new TurnMerge(game, turnNumber.Value)));

            logger.LogInformation("Calculating statistics");
            await EnusreSuccessAsync(mediator.Send(new TurnProcess(game, turnNumber.Value)));

            logger.LogInformation("Unlocking game");
            await gamesRep.UnlockAsync(gameId);

            await unit.CommitTransactionAsync();

            logger.LogInformation($"Turn {turnNumber} is ready");
        }
        catch (Exception ex) {
            logger.LogError(ex, ex.Message);

            await unit.RollbackTransactionAsync();

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
