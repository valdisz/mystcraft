namespace advisor {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using advisor.Features;
    using HotChocolate;
    using HotChocolate.AspNetCore.Authorization;
    using HotChocolate.Resolvers;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using MediatR;
    using Persistence;

    [InterfaceType("MutationResult")]
    public interface IMutationResult {
        bool IsSuccess { get; }
        string Error { get; }
    }

    public record MutationResult<T>(bool IsSuccess, T data, string Error) : IMutationResult;

    public record GameCreateRemoteResult(DbGame Game, bool IsSuccess, string Error) : IMutationResult;

    [Authorize]
    public class MutationType {
        [Authorize(Policy = Policies.UserManagers)]
        public Task<DbUser> CreateUser(IMediator mediator, string email, string password) {
            return mediator.Send(new UserCreate(email, password));
        }

        [Authorize(Policy = Policies.UserManagers)]
        public Task<DbUser> UpdateUserRoles(IMediator mediator, [ID("User")] long userId, string[] add, string[] remove) {
            return mediator.Send(new UserRolesUpdate(userId, add, remove));
        }

        [Authorize(Policy = Policies.GameMasters)]
        public async Task<DbGame> CreateLocalGame(IMediator mediator, string name, GameOptions options, IFile engine, IFile playerData, IFile gameData) {
            using var engineStream = engine.OpenReadStream();
            using var playersDataStream = playerData.OpenReadStream();
            using var gameDataStream = gameData.OpenReadStream();

            var result = await mediator.Send(new GameCreateLocal(name, engineStream, options, playersDataStream, gameDataStream));

            return result;
        }

        [Authorize(Policy = Policies.GameMasters)]
        public async Task<GameCreateRemoteResult> GameCreateRemote(IMediator mediator,
            string name, string engineVersion, string rulesetName, string rulesetVersion, GameOptions options
        ) {
            var result = await mediator.Send(new GameCreateRemote(name, engineVersion, rulesetName, rulesetVersion, options));

            return new GameCreateRemoteResult(
                result, true, null
            );
        }

        public Task<DbPlayer> GameJoin(IMediator mediator, [GlobalState] long currentUserId, [ID("Game")] long gameId) {
            return mediator.Send(new GameJoin(currentUserId, gameId));
        }

        [Authorize(Policy = Policies.GameMasters)]
        public Task<List<DbGame>> GameDelete(IMediator mediator, [ID("Game")] long gameId) {
            return mediator.Send(new GameDelete(gameId));
        }

        [Authorize(Policy = Policies.GameMasters)]
        public Task<DbGame> GameOptionsSet(IMediator mediator, [ID("Game")] long gameId, GameOptions options) {
            return mediator.Send(new GameOptionsSet(gameId, options));
        }

        [Authorize(Policy = Policies.GameMasters)]
        public Task<TurnReProcessResult> TurnReProcess(IMediator mediator, [ID("Game")] long gameId, int turn) {
            return mediator.Send(new TurnReProcess(gameId, turn));
        }

        public async Task<MutationResult<string>> SetOrders(
            IResolverContext context,
            IMediator mediator,
            Microsoft.AspNetCore.Authorization.IAuthorizationService auth,
            [ID("Unit")] string unitId,
            string orders) {
                var parsedId = DbUnit.ParseId(unitId);

                if (!await auth.AuthorizeOwnPlayerAsync(context.GetUser()!, parsedId.PlayerId)) {
                    context.ReportError(ErrorBuilder.New()
                        .SetMessage("You are not allowed to set orders for this Unit")
                        .SetCode(ErrorCodes.Authentication.NotAuthorized)
                        .SetPath(context.Path)
                        .AddLocation(context.Selection.SyntaxNode)
                        .Build());

                    return null;
                }

                try {
                    var result = await mediator.Send(new UnitOrdersSet(parsedId.PlayerId, parsedId.TurnNumber, parsedId.UnitNumber, orders));
                    return result == "Ok"
                        ? new MutationResult<string>(true, null, null)
                        : new MutationResult<string>(false, null, result);
                }
                catch (Exception ex) {
                    return new MutationResult<string>(false, null, ex.Message);
                }
            }

        // public Task<int> DeleteTurn([GlobalState] ClaimsPrincipal currentUser, [GraphQLType(typeof(RelayIdType))] string turnId) {
        //     return mediator.Send(new DeleteTurn(
        //         currentUser,
        //         ParseRelayId<long>("Turn", turnId)
        //     ));
        // }

        // [Authorize(Policy = Policies.GameMasters)]
        // public Task<DbGame> SetRuleset([GraphQLType(typeof(RelayIdType))] string gameId, string ruleset) {
        //     return mediator.Send(new SetRuleset(
        //         ParseRelayId<long>("Game", gameId),
        //         ruleset
        //     ));
        // }

        // todo: this is temp till player rights can be checked
        [Authorize(Policy = Policies.GameMasters)]
        public Task<AllianceCreateResult> AllianceCreate(IMediator mediator, [GlobalState] long currentUserId, [ID("Player")] long playerId, string name) {
            return mediator.Send(new AllianceCreate(currentUserId, playerId, name));
        }

        // todo: this is temp till player rights can be checked
        [Authorize(Policy = Policies.GameMasters)]
        public Task<AllianceJoinResult> AllianceJoin(IMediator mediator, [GlobalState] long currentUserId, [ID("Player")] long playerId, [ID("Alliance")] long allianceId) {
            return mediator.Send(new AllianceJoin(currentUserId, playerId, allianceId));
        }

        public Task<StudyPlanResult> StudyPlanTarget(IMediator mediator, [GlobalState] long currentUserId, [ID("Unit")] string unitId, string skill, int level) {
            return mediator.Send(new StudyPlanTarget(currentUserId, unitId, skill, level));
        }

        public Task<StudyPlanResult> StudyPlanStudy(IMediator mediator, [GlobalState] long currentUserId, [ID("Unit")] string unitId, string skill) {
            return mediator.Send(new StudPlanyStudy(currentUserId, unitId, skill));
        }

        public Task<StudyPlanResult> StudyPlanTeach(IMediator mediator, [GlobalState] long currentUserId, [ID("Unit")] string unitId, int[] units) {
            return mediator.Send(new StudyPlanTeach(currentUserId, unitId, units));
        }

        [Authorize(Policy = Policies.GameMasters)]
        public Task<PlayerQuitResult> PlayerQuit(IMediator mediator, [ID("Player")] long playerId) {
            return mediator.Send(new PlayerQuit(playerId));
        }
    }
}
