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

public record GameJoinRemoteResult(bool IsSuccess, string Error = null, DbPlayer Player = null) : IMutationResult;

public class GameJoinRemoteHandler : IRequestHandler<GameJoinRemote, GameJoinRemoteResult> {
    public GameJoinRemoteHandler(IUnitOfWork unit, IHttpClientFactory httpFactory, IMediator mediator) {
        this.unit = unit;
        this.games = unit.Games;
        this.httpFactory = httpFactory;
        this.mediator = mediator;
    }

    private readonly IUnitOfWork unit;
    private readonly IGameRepository games;
    private readonly IHttpClientFactory httpFactory;
    private readonly IMediator mediator;

    public async Task<GameJoinRemoteResult> Handle(GameJoinRemote request, CancellationToken cancellationToken) {
        var game = await games.GetOneNoTrackingAsync(request.GameId);
        if (game == null) {
            return new GameJoinRemoteResult(false, "Game does not exist.");
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
            await turnsRepo.AddReportAsync(
                player.Number,
                player.LastTurnNumber,
                Encoding.UTF8.GetBytes(reportText),
                cancellationToken
            );
        }
        catch (RepositoryException ex) {
            await unit.RollbackTransactionAsync(cancellationToken);
            return new GameJoinRemoteResult(false, ex.Message);
        }

        await unit.SaveChangesAsync(cancellationToken);

        await unit.CommitTransactionAsync(cancellationToken);

        return new GameJoinRemoteResult(true, Player: player);
    }
}
