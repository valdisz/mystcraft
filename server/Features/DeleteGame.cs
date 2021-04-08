namespace advisor.Features
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;

    public record DeleteGame(long GameId) : IRequest<List<DbGame>>;

    public record DeleteTurn(ClaimsPrincipal User, long TurnId) : IRequest<int>;

    public class DeleteGameHandler : IRequestHandler<DeleteGame, List<DbGame>>, IRequestHandler<DeleteTurn, int> {
        public DeleteGameHandler(Database db, IAuthorizationService auth) {
            this.db = db;
            this.auth = auth;
        }

        private readonly Database db;
        private readonly IAuthorizationService auth;

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
                    await DeleteTurnDependencies(turnId);
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

        public async Task<int> Handle(DeleteTurn request, CancellationToken cancellationToken) {
            var isGM = (await auth.AuthorizeAsync(request.User, Roles.GameMaster)).Succeeded;

            var turnId = request.TurnId;
            var turn = await db.FindAsync<DbTurn>(turnId);
            var playerId = turn.PlayerId;

            if (!isGM && !(await auth.AuthorizeOwnPlayer(request.User, playerId))) {
                return -1;
            }

            await DeleteTurnDependencies(turnId);
            db.Remove(turn);
            await db.SaveChangesAsync();


            var lastTurnNumber = db.Turns
                .Where(x => x.PlayerId == playerId)
                .Select(x => x.Number)
                .Max();

            var player = await db.Players.FindAsync(playerId);
            player.LastTurnNumber = lastTurnNumber;

            await db.SaveChangesAsync();

            return lastTurnNumber;
        }

        private async Task DeleteTurnDependencies(long turnId) {
            await DeleteFromTurnAsync<DbStat>(turnId);
            await DeleteFromTurnAsync<DbStudyPlan>(turnId);
            await DeleteFromTurnAsync<DbEvent>(turnId);
            await DeleteFromTurnAsync<DbUnit>(turnId);
            await DeleteFromTurnAsync<DbStructure>(turnId);
            await DeleteFromTurnAsync<DbRegion>(turnId);
            await DeleteFromTurnAsync<DbFaction>(turnId);
            await DeleteFromTurnAsync<DbReport>(turnId);
        }

        private Task DeleteFromTurnAsync<T>(long turnId) {
            var targetTable = db.Model.FindEntityType(typeof(T)).GetTableName();

            return db.Database.ExecuteSqlRawAsync($@"delete from {targetTable} where TurnId = {turnId}");
        }
    }
}
