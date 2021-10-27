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

    public record DeleteTurn(ClaimsPrincipal User, long PlayerId, int TurnNumber) : IRequest<int>;

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
                var players = await db.Players
                    .AsNoTracking()
                    .Select(x => x.Id)
                    .ToListAsync();

                foreach (var playerId in players) {
                    await DeleteTurnDependenciesAsync("PlayerId", playerId);
                    await DeleteFromTableAsync<DbPlayer>("Id", playerId);
                }

                db.Remove(game);
                await db.SaveChangesAsync();
            }

            var games = await db.Games.ToListAsync();
            return games;
        }

        public async Task<int> Handle(DeleteTurn request, CancellationToken cancellationToken) {
            var isGM = (await auth.AuthorizeAsync(request.User, Roles.GameMaster)).Succeeded;
            var playerId = request.PlayerId;

            if (!isGM && !(await auth.AuthorizeOwnPlayerAsync(request.User, playerId))) {
                return -1;
            }

            await DeleteTurnDependenciesAsync(playerId, request.TurnNumber);

            var turn = await db.Turns.FirstOrDefaultAsync(x => x.PlayerId == playerId && x.Number == request.TurnNumber);
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

        private async Task DeleteTurnDependenciesAsync(string field, long id) {
            await DeleteFromTableAsync<DbStat>(field, id);
            await DeleteFromTableAsync<DbStudyPlan>(field, id);
            await DeleteFromTableAsync<DbEvent>(field, id);
            await DeleteFromTableAsync<DbUnit>(field, id);
            await DeleteFromTableAsync<DbStructure>(field, id);
            await DeleteFromTableAsync<DbRegion>(field, id);
            await DeleteFromTableAsync<DbFaction>(field, id);
            await DeleteFromTableAsync<DbReport>(field, id);
            await DeleteFromTableAsync<DbTurn>(field, id);
        }

        private async Task DeleteTurnDependenciesAsync(long playerId, int turnNumber) {
            await DeleteFromTableAsync<DbStat>("PlayerId", playerId, "TurnNumber", turnNumber);
            await DeleteFromTableAsync<DbStudyPlan>("PlayerId", playerId, "TurnNumber", turnNumber);
            await DeleteFromTableAsync<DbEvent>("PlayerId", playerId, "TurnNumber", turnNumber);
            await DeleteFromTableAsync<DbUnit>("PlayerId", playerId, "TurnNumber", turnNumber);
            await DeleteFromTableAsync<DbStructure>("PlayerId", playerId, "TurnNumber", turnNumber);
            await DeleteFromTableAsync<DbRegion>("PlayerId", playerId, "TurnNumber", turnNumber);
            await DeleteFromTableAsync<DbFaction>("PlayerId", playerId, "TurnNumber", turnNumber);
            await DeleteFromTableAsync<DbReport>("PlayerId", playerId, "TurnNumber", turnNumber);
            await DeleteFromTableAsync<DbTurn>("PlayerId", playerId, "TurnNumber", turnNumber);
        }

        private Task DeleteFromTableAsync<T>(string field, long id) {
            var targetTable = db.Model.FindEntityType(typeof(T)).GetTableName();

            return db.Database.ExecuteSqlRawAsync($@"delete from {targetTable} where {field} = {id}");
        }

        private Task DeleteFromTableAsync<T>(string field1, long id1, string field2, long id2) {
            var targetTable = db.Model.FindEntityType(typeof(T)).GetTableName();

            return db.Database.ExecuteSqlRawAsync($@"delete from {targetTable} where {field1} = {id1} and {field2} = {id2}");
        }
    }
}
