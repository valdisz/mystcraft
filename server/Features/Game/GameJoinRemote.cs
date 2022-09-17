namespace advisor.Features;

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using advisor.Model;
using advisor.Persistence;
using advisor.Remote;
using advisor.Schema;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public record GameJoinRemote(long UserId, long GameId, long PlayerId, string Password) : IRequest<GameJoinRemoteResult>;

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
        var playersRepo = unit.Players(game);
        var turnsRepo = unit.Turns(game);

        var player = await playersRepo.GetOneNoTrackingAsync(request.PlayerId, cancellationToken);
        if (player == null) {
            return new GameJoinRemoteResult(false, "Player does not exist.");
        }

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

        JReport report = null;
        string json = null;
        string error = null;
        try {
            (report, json) = await ParseReportAsync(reportText);
        }
        catch (Exception ex) {
            error = ex.ToString();
        }

        try {
            player = await playersRepo.ClamFactionAsync(request.UserId, request.PlayerId, request.Password, cancellationToken);

            var name = report?.Faction?.Name;
            if (name != null) {
                player.Name = name;

                var t0 = await playersRepo.GetPlayerTurnAsync(player.Id, player.LastTurnNumber, cancellationToken);
                var t1 = await playersRepo.GetPlayerTurnAsync(player.Id, player.NextTurnNumber.Value, cancellationToken);

                t0.Name = name;
                t1.Name = name;
            }

            var r = await turnsRepo.AddReportAsync(
                player.Number,
                player.LastTurnNumber,
                Encoding.UTF8.GetBytes(reportText),
                cancellationToken
            );

            if (json != null) {
                r.Json = Encoding.UTF8.GetBytes(json);
                r.Parsed = true;
            }
            else {
                r.Error = error;
            }
        }
        catch (PlayersRepositoryException ex) {
            return new GameJoinRemoteResult(false, ex.Message);
        }

        await unit.SaveChangesAsync(cancellationToken);

        return new GameJoinRemoteResult(true, Player: player);
    }

    private async Task<(JReport, string)> ParseReportAsync(string source) {
        using var textReader = new StringReader(source);

        using var atlantisReader = new AtlantisReportJsonConverter(textReader);
        JObject json = await atlantisReader.ReadAsJsonAsync();
        JReport report = json.ToObject<JReport>();

        var jsonText = json.ToString(Formatting.None);

        return (report, jsonText);
    }
}
