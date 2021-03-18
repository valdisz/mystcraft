namespace advisor.Features
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record DeleteGame(long GameId) : IRequest<List<DbGame>> {

    }

    public class DeleteGameHandler : IRequestHandler<DeleteGame, List<DbGame>> {
        public DeleteGameHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<List<DbGame>> Handle(DeleteGame request, CancellationToken cancellationToken) {
            var gameId = request.GameId;
            var game = await db.Games.FindAsync(gameId);

            if (game != null)  {
                var turns = await db.Turns
                    .AsNoTracking()
                    .Include(x => x.Player)
                    .Where(x => x.Player.GameId == gameId)
                    .Select(x => x.Id)
                    .ToListAsync();

                foreach (var turnId in turns) {
                    await DeleteFromTurnAsync<DbStudyPlan>(turnId);
                    await DeleteFromTurnAsync<DbEvent>(turnId);
                    await DeleteFromTurnAsync<DbUnit>(turnId);
                    await DeleteFromTurnAsync<DbStructure>(turnId);
                    await DeleteFromTurnAsync<DbRegion>(turnId);
                    await DeleteFromTurnAsync<DbFaction>(turnId);
                    await DeleteFromTurnAsync<DbReport>(turnId);
                }

                var players = await db.Players
                    .AsNoTracking()
                    .Where(x => x.GameId == gameId)
                    .Select(x => x.Id)
                    .ToListAsync();

                var membershipTable = db.Model.FindEntityType(typeof(DbUniversityMembership)).GetTableName();
                foreach (var playerId in players) {
                    await db.Database.ExecuteSqlRawAsync($@"delete from {membershipTable} where {nameof(DbUniversityMembership.PlayerId)} = {playerId}");
                }

                db.Remove(game);
                await db.SaveChangesAsync();
            }

            var games = await db.Games.ToListAsync();
            return games;
        }

        private Task DeleteFromTurnAsync<T>(long turnId) {
            var targetTable = db.Model.FindEntityType(typeof(T)).GetTableName();

            return db.Database.ExecuteSqlRawAsync($@"delete from {targetTable} where TurnId = {turnId}");
        }
    }
}
