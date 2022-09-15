namespace advisor.Features;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Remote;
using advisor.Schema;
using MediatR;

public record GameJoinRemote(long GameId, long UserId, long PlayerId, string Password) : IRequest<GameJoinRemoteResult>;

public record GameJoinRemoteResult(bool IsSuccess, string Error = null, DbPlayer Player = null) : IMutationResult;

public class GameJoinRemoteHandler : IRequestHandler<GameJoinRemote, GameJoinRemoteResult> {
    public GameJoinRemoteHandler(IUnitOfWork unit, IHttpClientFactory httpFactory) {
        this.unit = unit;
        this.games = unit.Games;
        this.httpFactory = httpFactory;
    }

    private readonly IUnitOfWork unit;
    private readonly IGameRepository games;
    private readonly IHttpClientFactory httpFactory;

    public async Task<GameJoinRemoteResult> Handle(GameJoinRemote request, CancellationToken cancellationToken) {
        var game = await games.GetOneNoTrackingAsync(request.GameId);
        if (game == null) {
            return new GameJoinRemoteResult(false, "Game does not exist.");
        }

        var remote = new NewOriginsClient(game.Options.ServerAddress, httpFactory);
        var players = unit.Players(game);

        var player = await players.GetOneNoTrackingAsync(request.PlayerId, cancellationToken);
        if (player == null) {
            return new GameJoinRemoteResult(false, "Player does not exist.");
        }

        if (!player.Number.HasValue) {
            return new GameJoinRemoteResult(false, "Player does not have faction number yet.");
        }

        string reportText;
        try {
            reportText = await remote.DownloadReportAsync(player.Number.Value, request.Password, cancellationToken);
        }
        catch (WrongFactionOrPasswordException) {
            return new GameJoinRemoteResult(false, "Wrong faction number or password.");
        }
        catch (Exception) {
            return new GameJoinRemoteResult(false, "Cannot reach the remote server.");
        }

        try {
            player = await players.ClamFactionAsync(reportText, request.UserId, request.PlayerId, request.Password, cancellationToken);
        }
        catch (PlayersRepositoryException ex) {
            return new GameJoinRemoteResult(false, ex.Message);
        }

        await unit.SaveChangesAsync(cancellationToken);

        return new GameJoinRemoteResult(true, Player: player);
    }
}
