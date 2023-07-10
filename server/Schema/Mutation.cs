namespace advisor.Schema;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using MediatR;
using advisor.Features;
using advisor.Persistence;
using advisor.Model;
using System.Collections.Generic;

[Authorize]
public class Mutation {
    [Authorize(Policy = Policies.UserManagers)]
    public Task<DbUser> UserCreate(IResolverContext context, IMediator mediator, string email, string password) {
        return mediator.Send(new UserCreate(email, password), context.RequestAborted);
    }

    [Authorize(Policy = Policies.UserManagers)]
    public Task<DbUser> UserRolesUpdate(IResolverContext context, IMediator mediator, [ID("User")] long userId, string[] add, string[] remove) {
        return mediator.Send(new UserRolesUpdate(userId, add, remove), context.RequestAborted);
    }

    [Authorize(Policy = Policies.GameMasters)]
    public async Task<GameEngineCreateResult> GameEngineCreate(IResolverContext context, IMediator mediator, string name, IFile content, IFile ruleset) {
        using var cs = content.OpenReadStream();
        using var rs = ruleset.OpenReadStream();

        var result = await mediator.Send(new GameEngineCreate(name, cs, rs), context.RequestAborted);

        return result;
    }

    [Authorize(Policy = Policies.GameMasters)]
    public Task<GameEngineDeleteResult> GameEngineDelete(IResolverContext context, IMediator mediator, [ID(GameEngineType.NAME)] long gameEngineId) =>
        mediator.Send(new GameEngineDelete(gameEngineId), context.RequestAborted);

    [Authorize(Policy = Policies.GameMasters)]
    public Task<GameCreateResult> GameCreate(
        IMediator mediator,
        string name,
        [ID(GameEngineType.NAME)] long gameEngineId,
        List<MapLevel> map,
        string schedule,
        string timeZone,
        DateTimeOffset? startAt,
        DateTimeOffset? finishAt
        // IFile playerData,
        // IFile gameData
    ) {
        // FIXME
        // using var playersDataStream = playerData.OpenReadStream();
        // using var gameDataStream = gameData.OpenReadStream();

        return mediator.Send(new GameCreate(name, gameEngineId, map, schedule, timeZone, startAt, finishAt));
    }

    // [Authorize(Policy = Policies.GameMasters)]
    // public Task<GameCreateRemoteResult> GameCreateRemote(IMediator mediator, string name, GameOptions options) {
    //     return mediator.Send(new GameCreateRemote(name, options));
    // }

    public Task<GameJoinLocalResult> GameJoinLocal(IMediator mediator, [GlobalState] long currentUserId, [ID(GameType.NAME)] long gameId, string name) {
        return mediator.Send(new GameJoinLocal(currentUserId, gameId, name));
    }

    public Task<GameJoinRemoteResult> GameJoinRemote(IMediator mediator, [GlobalState] long currentUserId, [ID(GameType.NAME)] long gameId, [ID("Player")] long playerId, string password) {
        return mediator.Send(new GameJoinRemote(currentUserId, gameId, playerId, password));
    }

    [Authorize(Policy = Policies.GameMasters)]
    public Task<GameStartResult> GameStart(IMediator mediator, [ID(GameType.NAME)] long gameId) {
        return mediator.Send(new GameStart(gameId));
    }

    [Authorize(Policy = Policies.GameMasters)]
    public Task<GamePauseResult> GamePause(IMediator mediator, [ID(GameType.NAME)] long gameId) {
        return mediator.Send(new GamePause(gameId));
    }

    [Authorize(Policy = Policies.GameMasters)]
    public Task<GameStopResult> GameComplete(IMediator mediator, [ID(GameType.NAME)] long gameId) {
        return mediator.Send(new GameStop(gameId));
    }

    [Authorize(Policy = Policies.GameMasters)]
    public Task<GameNextTurnResult> GameNextTurn(IMediator mediator, [ID(GameType.NAME)] long gameId, int? turnNumber = null, GameNextTurnForceInput force = null) {
        return mediator.Send(new GameNextTurn(gameId, turnNumber, force));
    }

    // FIXME
    // [Authorize(Policy = Policies.GameMasters)]
    // public Task<List<DbGame>> GameDelete(IMediator mediator, [ID("Game")] long gameId) {
    //     return mediator.Send(new GameDelete(gameId));
    // }

    [Authorize(Policy = Policies.GameMasters)]
    public Task<GameOptionsSetResult> GameOptionsSet(IMediator mediator, [ID(GameType.NAME)] long gameId, GameOptions options) {
        return mediator.Send(new GameOptionsSet(gameId, options));
    }

    [Authorize(Policy = Policies.GameMasters)]
    public Task<GameScheduleSetResult> GameScheduleSet(IMediator mediator, [ID(GameType.NAME)] long gameId, string schedule) {
        return mediator.Send(new GameScheduleSet(gameId, schedule));
    }

    // FIXME
    // public Task<GameTurnRunResult> GameTurnRun(IMediator mediator, [ID("Game")] long gameId) {
    //     return mediator.Send(new GameTurnRun(gameId));
    // }

    // FIXME
    // [Authorize(Policy = Policies.GameMasters)]
    // public Task<TurnReProcessResult> TurnReProcess(IMediator mediator, [ID("Game")] long gameId, int turn) {
    //     return mediator.Send(new TurnReProcess(gameId, turn));
    // }

    [Authorize(Policy = Policies.OwnPlayer)]
    public async Task<UnitOrdersSetResult> SetOrders(IResolverContext context, IMediator mediator, [ID("Unit")] string unitId, string orders) {
        try {
            var parsedId = DbUnit.ParseId(unitId);
            var result = await mediator.Send(new UnitOrdersSet(parsedId.PlayerId, parsedId.TurnNumber + 1, parsedId.UnitNumber, orders));
            return result;
        }
        catch (Exception ex) {
            return new UnitOrdersSetResult(false, ex.Message);
        }
    }

    // FIXME
    public Task<int> DeleteTurn([GlobalState] ClaimsPrincipal currentUser, [ID("PlayerTurn")] string turnId) {
        // return mediator.Send(new DeleteTurn(
        //     currentUser,
        //     ParseRelayId<long>("Turn", turnId)
        // ));
        throw new NotImplementedException();
    }

    // FIXME
    [Authorize(Policy = Policies.GameMasters)]
    public Task<DbGame> SetRuleset([ID("Game")] long gameId, string ruleset) {
        // return mediator.Send(new SetRuleset(
        //     ParseRelayId<long>("Game", gameId),
        //     ruleset
        // ));
        throw new NotImplementedException();
    }

    // FIXME
    // TODO: this is temp till player rights can be checked
    [Authorize(Policy = Policies.GameMasters)]
    public Task<AllianceCreateResult> AllianceCreate(IMediator mediator, [GlobalState] long currentUserId, [ID("Player")] long playerId, string name) {
        // return mediator.Send(new AllianceCreate(currentUserId, playerId, name));
        throw new NotImplementedException();
    }

    // FIXME
    // TODO: this is temp till player rights can be checked
    [Authorize(Policy = Policies.GameMasters)]
    public Task<AllianceJoinResult> AllianceJoin(IMediator mediator, [GlobalState] long currentUserId, [ID("Player")] long playerId, [ID("Alliance")] long allianceId) {
        // return mediator.Send(new AllianceJoin(currentUserId, playerId, allianceId));
        throw new NotImplementedException();
    }

    // FIXME
    public Task<StudyPlanResult> StudyPlanTarget(IMediator mediator, [GlobalState] long currentUserId, [ID("Unit")] string unitId, string skill, int level) {
        // return mediator.Send(new StudyPlanTarget(currentUserId, unitId, skill, level));
        throw new NotImplementedException();
    }

    // FIXME
    public Task<StudyPlanResult> StudyPlanStudy(IMediator mediator, [GlobalState] long currentUserId, [ID("Unit")] string unitId, string skill) {
        // return mediator.Send(new StudPlanyStudy(currentUserId, unitId, skill));
        throw new NotImplementedException();
    }

    // FIXME
    public Task<StudyPlanResult> StudyPlanTeach(IMediator mediator, [GlobalState] long currentUserId, [ID("Unit")] string unitId, int[] units) {
        // return mediator.Send(new StudyPlanTeach(currentUserId, unitId, units));
        throw new NotImplementedException();
    }

    // FIXME
    [Authorize(Policy = Policies.GameMasters)]
    public Task<PlayerQuitResult> PlayerQuit(IMediator mediator, [ID("Player")] long playerId) {
        // return mediator.Send(new PlayerQuit(playerId));
        throw new NotImplementedException();
    }
}
