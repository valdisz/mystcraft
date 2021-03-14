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

        [Authorize(Policy = Policies.UserManagers)]
        public Task<DbUser> CreateUser(string email, string password) {
            return mediator.Send(new CreateUser(email, password));
        }

        [Authorize(Policy = Policies.UserManagers)]
        public Task<DbUser> UpdateUserRoles([GraphQLType(typeof(RelayIdType))] string userId, string[] add, string[] remove) {
            return mediator.Send(new UpdateUserRoles(
                ParseRelayId<long>("User", userId),
                add,
                remove
            ));
        }

        [Authorize(Policy = Policies.GameMasters)]
        public Task<DbGame> CreateGame(string name) {
            return mediator.Send(new CreateGame(name));
        }

        public Task<DbPlayer> JoinGame([GlobalState] long currentUserId, [GraphQLType(typeof(RelayIdType))] string gameId) {
            return mediator.Send(new JoinGame(
                currentUserId,
                ParseRelayId<long>("Game", gameId)
            ));
        }

        public Task<DbUniversity> OpenUniversity([GlobalState] long currentUserId, [GraphQLType(typeof(RelayIdType))] string playerId, string name) {
            return mediator.Send(new OpenUniversity(
                currentUserId,
                ParseRelayId<long>("Player", playerId),
                name
            ));
        }

        public Task<DbUniversity> JoinUniversity([GlobalState] long currentUserId, [GraphQLType(typeof(RelayIdType))] string universityId, [GraphQLType(typeof(RelayIdType))] string playerId) {
            return mediator.Send(new JoinUniversity(
                currentUserId,
                ParseRelayId<long>("Player", playerId),
                ParseRelayId<long>("University", universityId)
            ));
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
