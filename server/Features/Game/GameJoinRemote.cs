namespace advisor.Features;

using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Remote;
using advisor.Schema;
using MediatR;

public record GameJoinRemote(long UserId, long GameId, long PlayerId, string Password) : IRequest<GameJoinRemoteResult>;

public record GameJoinRemoteResult(bool IsSuccess, string Error = null, DbPlayer Player = null) : MutationResult(IsSuccess, Error);

public class GameJoinRemoteHandler : IRequestHandler<GameJoinRemote, GameJoinRemoteResult> {
    public GameJoinRemoteHandler(IGameRepository games, IUnitOfWork unit, IHttpClientFactory httpFactory, IMediator mediator) {
        this.unit = unit;
        this.games = games;
        this.httpFactory = httpFactory;
        this.mediator = mediator;
    }

    private readonly IUnitOfWork unit;
    private readonly IGameRepository games;
    private readonly IHttpClientFactory httpFactory;
    private readonly IMediator mediator;

    public async Task<GameJoinRemoteResult> Handle(GameJoinRemote request, CancellationToken cancellationToken) {
        var game = await games.GetOneAsync(request.GameId, withTracking: false);

        switch (game?.Status) {
            case null: return new GameJoinRemoteResult(false, "Game does not exist.");
            case GameStatus.NEW: return new GameJoinRemoteResult(false, "Game not yet started.");
            case GameStatus.PAUSED: return new GameJoinRemoteResult(false, "Game paused.");
            case GameStatus.LOCKED: return new GameJoinRemoteResult(false, "Game is processing turn.");
            case GameStatus.COMPLEATED: return new GameJoinRemoteResult(false, "Game compleated.");
        }

        var playersRepo = unit.Players(game);
        var player = await playersRepo.GetOneAsync(request.PlayerId);
        if (player == null) {
            return new GameJoinRemoteResult(false, "Player does not exist.");
        }

        var remote = new NewOriginsClient(game.Options.ServerAddress, httpFactory);
        string reportText;
        try {
            reportText = await remote.DownloadReportAsync(player.Number, request.Password, cancellationToken);
        }
        catch (WrongFactionOrPasswordException) {
            return new GameJoinRemoteResult(false, "Wrong faction number or password.");
        }
        catch (Exception) {
            return new GameJoinRemoteResult(false, "Cannot reach the remote server.");
        }

        await unit.BeginTransactionAsync(cancellationToken);

        var turnsRepo = unit.Turns(game);
        try {
            player = await playersRepo.ClamFactionAsync(request.UserId, request.PlayerId, request.Password, cancellationToken);
            var report = await turnsRepo.AddReportAsync(
                request.PlayerId,
                player.Number,
                player.LastTurnNumber.Value,
                Encoding.UTF8.GetBytes(reportText),
                cancellationToken
            );
        }
        catch (RepositoryException ex) {
            await unit.RollbackTransactionAsync(cancellationToken);
            return new GameJoinRemoteResult(false, ex.Message);
        }

        await unit.SaveChangesAsync(cancellationToken);

        if (player.LastTurnNumber.HasValue) {
            var turnNumber = player.LastTurnNumber.Value;
            long[] playerIds = { player.Id };

            MutationResult result;

            result = await mediator.Send(new TurnParse(game, turnNumber, PlayerIds: playerIds));
            if (!result) {
                await unit.RollbackTransactionAsync(cancellationToken);
                return new GameJoinRemoteResult(result, result.Error);
            }

            result = await mediator.Send(new TurnMerge(game, turnNumber, PlayerIds: playerIds));
            if (!result) {
                await unit.RollbackTransactionAsync(cancellationToken);
                return new GameJoinRemoteResult(result, result.Error);
            }

            result = await mediator.Send(new TurnProcess(game, turnNumber, PlayerIds: playerIds));
            if (!result) {
                await unit.RollbackTransactionAsync(cancellationToken);
                return new GameJoinRemoteResult(result, result.Error);
            }
        }

        await unit.CommitTransactionAsync(cancellationToken);
        return new GameJoinRemoteResult(true, Player: player);
    }
}
