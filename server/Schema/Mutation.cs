namespace atlantis
{
    using System.Threading.Tasks;
    using atlantis.Features;
    using HotChocolate.AspNetCore.Authorization;
    using HotChocolate.Types.Relay;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    [Authorize]
    public class Mutation {
        public Mutation(Database db, IMediator mediator, IIdSerializer idSerializer) {
            this.db = db;
            this.mediator = mediator;
            this.idSerializer = idSerializer;
        }

        private readonly Database db;
        private readonly IMediator mediator;
        private readonly IIdSerializer idSerializer;

        [Authorize(Policy = Roles.UserManager)]
        public Task<DbUser> CreateUser(string email, string password) {
            return mediator.Send(new CreateUser(email, password));
        }

        [Authorize(Policy = Roles.UserManager)]
        public Task<DbUser> UpdateUserRoles(string userId, string[] add, string[] remove) {
            var id = idSerializer.Deserialize(userId);
            if (id.TypeName != "User") return null;

            return mediator.Send(new UpdateUserRoles((long) id.Value, add, remove));
        }

        [Authorize(Policy = Roles.GameMaster)]
        public async Task<DbGame> CreateGame(string name) {
            var newGame = new DbGame  {
                Name = name
            };

            await db.Games.AddAsync(newGame);
            await db.SaveChangesAsync();

            return newGame;
        }

        [Authorize(Policy = Roles.GameMaster)]
        public async Task<string> DeleteGame(long id)  {
            var game = await db.Games.FirstOrDefaultAsync(x => x.Id == id);
            if (game != null) {
                db.Games.Remove(game);
                await db.SaveChangesAsync();

                return "deleted";
            }

            return "not found";
        }
    }
}
