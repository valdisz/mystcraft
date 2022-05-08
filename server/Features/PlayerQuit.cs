namespace advisor.Features {
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record PlayerQuit(long playerId) : IRequest<PlayerQuitResult>;
    public record PlayerQuitResult(bool IsSuccess, string Error) : IMutationResult;

    public class PlayerQuitHandler : IRequestHandler<PlayerQuit, PlayerQuitResult> {
        public PlayerQuitHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<PlayerQuitResult> Handle(PlayerQuit request, CancellationToken cancellationToken) {
            var player = await db.Players.SingleOrDefaultAsync(x => x.Id == request.playerId);
            player.IsQuit = true;
            await db.SaveChangesAsync();

            return new PlayerQuitResult(true, null);
        }
    }
}
