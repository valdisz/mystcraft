namespace advisor.Features {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record GameJoinLocal(long UserId, long GameId, string Name) : IRequest<GameJoinLocalResult>;

    public record GameJoinLocalResult(bool IsSuccess, string Error = null, DbPlayer Player = null) : IMutationResult;

    public class GameJoinLocalHandler : IRequestHandler<GameJoinLocal, GameJoinLocalResult> {
        public GameJoinLocalHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<GameJoinLocalResult> Handle(GameJoinLocal request, CancellationToken cancellationToken) {
            var game = await db.Games.FindAsync(request.GameId);
            if (game == null) {
                return new GameJoinLocalResult(false, "Game does not exist.");
            }

            var player = db.Players
                .AsNoTracking()
                .InGame(game)
                .OnlyActivePlayers()
                .SingleOrDefault(x => x.UserId == request.UserId);

            if (player != null) {
                return new GameJoinLocalResult(false, "There already is an active player in this game.");
            }

            player = new DbPlayer {
                GameId = request.GameId,
                UserId = request.UserId,
                Name = request.Name
            };

            await db.Players.AddAsync(player);
            await db.SaveChangesAsync();

            return new GameJoinLocalResult(true, Player: player);
        }
    }
}
