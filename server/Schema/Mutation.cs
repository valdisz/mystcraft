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

    [Authorize]
    public class MutationType {
        [Authorize(Policy = Policies.UserManagers)]
        public Task<DbUser> CreateUser(IMediator mediator, string email, string password) {
            return mediator.Send(new CreateUser(email, password));
        }

        [Authorize(Policy = Policies.UserManagers)]
        public Task<DbUser> UpdateUserRoles(IMediator mediator, [ID("User")] long userId, string[] add, string[] remove) {
            return mediator.Send(new UpdateUserRoles(userId, add, remove));
        }

        [Authorize(Policy = Policies.GameMasters)]
        public async Task<DbGame> CreateLocalGame(IMediator mediator, string name, GameOptions options, IFile engine, IFile playerData, IFile gameData) {
            using var engineStream = engine.OpenReadStream();
            using var playersDataStream = playerData.OpenReadStream();
            using var gameDataStream = gameData.OpenReadStream();

            var result = await mediator.Send(new CreateLocalGame(name, engineStream, options, playersDataStream, gameDataStream));

            return result;
        }

        public Task<DbPlayer> JoinGame(IMediator mediator, [GlobalState] long currentUserId, [ID("Game")] long gameId) {
            return mediator.Send(new JoinGame(currentUserId, gameId));
        }

        [Authorize(Policy = Policies.GameMasters)]
        public Task<List<DbGame>> DeleteGame(IMediator mediator, [ID("Game")] long gameId) {
            return mediator.Send(new DeleteGame(gameId));
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
                    var result = await mediator.Send(new SetOrders(parsedId.PlayerId, parsedId.TurnNumber, parsedId.UnitNumber, orders));
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
        // public Task<DbGame> SetGameOptions([GraphQLType(typeof(RelayIdType))] string gameId, GameOptions options) {
        //     return mediator.Send(new SetGameOptions(
        //         ParseRelayId<long>("Game", gameId),
        //         options
        //     ));
        // }

        // [Authorize(Policy = Policies.GameMasters)]
        // public Task<DbGame> SetRuleset([GraphQLType(typeof(RelayIdType))] string gameId, string ruleset) {
        //     return mediator.Send(new SetRuleset(
        //         ParseRelayId<long>("Game", gameId),
        //         ruleset
        //     ));
        // }

        // public Task<DbAlliance> OpenUniversity([GlobalState] long currentUserId, [GraphQLType(typeof(RelayIdType))] string playerId, string name) {
        //     return mediator.Send(new OpenUniversity(
        //         currentUserId,
        //         ParseRelayId<long>("Player", playerId),
        //         name
        //     ));
        // }

        // public Task<DbAlliance> JoinUniversity([GlobalState] long currentUserId, [GraphQLType(typeof(RelayIdType))] string universityId, [GraphQLType(typeof(RelayIdType))] string playerId) {
        //     return mediator.Send(new JoinUniversity(
        //         currentUserId,
        //         ParseRelayId<long>("Player", playerId),
        //         ParseRelayId<long>("University", universityId)
        //     ));
        // }

        // public Task<DbStudyPlan> SetStudyPlanTarget(
        //     [GlobalState] long currentUserId,
        //     [GraphQLType(typeof(RelayIdType))] string studyPlanId,
        //     string skill,
        //     int level) {
        //         return mediator.Send(new SetStudyPlanTarget(
        //             currentUserId,
        //             ParseRelayId<long>("StudyPlan", studyPlanId),
        //             skill,
        //             level
        //         ));
        //     }

        // public Task<DbStudyPlan> SetStudPlanyStudy(
        //     [GlobalState] long currentUserId,
        //     [GraphQLType(typeof(RelayIdType))] string studyPlanId,
        //     string skill) {
        //         return mediator.Send(new SetStudPlanyStudy(
        //             currentUserId,
        //             ParseRelayId<long>("StudyPlan", studyPlanId),
        //             skill
        //         ));
        //     }

        // public Task<DbStudyPlan> SetStudyPlanTeach(
        //     [GlobalState] long currentUserId,
        //     [GraphQLType(typeof(RelayIdType))] string studyPlanId,
        //     long[] units) {
        //         return mediator.Send(new SetStudyPlanTeach(
        //             currentUserId,
        //             ParseRelayId<long>("StudyPlan", studyPlanId),
        //             units
        //         ));
        //     }
    }
}
