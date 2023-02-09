namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;

public record PlayerQuit(long GameId, long PlayerId) : IRequest<PlayerQuitResult>;
public record PlayerQuitResult(bool IsSuccess, string Error = null, DbPlayer Player = null) : IMutationResult;

public class PlayerQuitHandler : IRequestHandler<PlayerQuit, PlayerQuitResult> {
    public PlayerQuitHandler(IUnitOfWork unit) {
        this.unit = unit;
    }

    private readonly IUnitOfWork unit;

    public async Task<PlayerQuitResult> Handle(PlayerQuit request, CancellationToken cancellationToken) {
        var players = await unit.PlayersAsync(request.GameId, cancellationToken);

        var player = await players.GetOneAsync(request.PlayerId);
        if (player == null) {
            return new PlayerQuitResult(false, "Player not found.");
        }

        player.IsQuit = true;

        await unit.SaveChanges(cancellationToken);

        return new PlayerQuitResult(true, Player: player);
    }
}
