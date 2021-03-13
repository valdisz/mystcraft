namespace advisor
{
    using System;
    using System.Threading.Tasks;
    using advisor.Features;
    using HotChocolate;
    using HotChocolate.AspNetCore.Authorization;
    using HotChocolate.Types.Relay;
    using MediatR;
    using Persistence;

    using RelayIdType = HotChocolate.Types.NonNullType<HotChocolate.Types.IdType>;

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
        public Task<DbUser> UpdateUserRoles([GraphQLType(typeof(RelayIdType))] string userId, string[] add, string[] remove) {
            var id = ParseRelayId<long>("User", userId);

            return mediator.Send(new UpdateUserRoles(id, add, remove));
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

        public Task<DbUserGame> JoinGame([GlobalState] long currentUserId, [GraphQLType(typeof(RelayIdType))] string gameId) {
            var id = ParseRelayId<long>("Game", gameId);

            return mediator.Send(new JoinGame(id, currentUserId));
        }

        private T ParseRelayId<T>(string typeName, string value) {
            var id = idSerializer.Deserialize(value);

            if (id.TypeName != typeName) {
                throw new ArgumentException($"Expected ID of type {typeName}, but got {id.TypeName}");
            }

            return (T) Convert.ChangeType(id.Value, typeof(T));
        }
    }
}
