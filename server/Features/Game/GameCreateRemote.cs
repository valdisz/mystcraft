namespace advisor.Features;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using advisor.Model;
using MediatR;

public record GameCreateRemote(
    string Name,
    long GameEngineId,
    // Stream Ruleset,
    List<MapLevel> Levels,
    string Schedule,
    string TimeZone,
    DateTimeOffset? StartAt,
    DateTimeOffset? FinishAt
) : IRequest<GameCreateRemoteResult>;

public record GameCreateRemoteResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error) {
    public static GameCreateRemoteResult New(Validation<Error, DbGame> result) =>
        result.Match(
            Succ: g => new GameCreateRemoteResult(true, Game: g),
            Fail: e => new GameCreateRemoteResult(false, Error: e.Head.Message)
        );
}

public class GameCreateRemoteHandler : IRequestHandler<GameCreateRemote, GameCreateRemoteResult> {
    public GameCreateRemoteHandler(Database database) {
        this.database = database;
    }

    private readonly Database database;

    public Task<GameCreateRemoteResult> Handle(GameCreateRemote request, CancellationToken cancellationToken) =>
        Validate(request)
            .Map(GameInterpreter<Runtime>.Interpret)
            .Unwrap(Runtime.New(database, cancellationToken))
            .Map(GameCreateRemoteResult.New);

    private static Validation<Error, Mystcraft<DbGame>> Validate(GameCreateRemote request) =>
    (
        ValidateName(request.Name).ForField("Name"),
        GameEngineId.New(request.GameEngineId).ForField(nameof(GameCreateRemote.GameEngineId)),
        ValidateMap(request.Levels).ForField("Levels"),
        GameSchedule.New(request.Schedule, request.TimeZone).ForField("Schedule"),
        GamePeriod.New(Optional(request.StartAt), Optional(request.FinishAt)).ForField("Period")
    )
    .Apply(Mystcraft.CreateGameRemote);

    private static Validation<Error, string> ValidateName(string name) =>
        NotEmpty(name)
            .Bind(WithinLength(1, Some(Size.LABEL)));

    private static Validation<Error, Stream> ValidateRuleset(Stream ruleset) =>
        Optional(ruleset)
            .ToValidation(Error.New("Required."));

    private static Validation<Error, List<MapLevel>> ValidateMap(List<MapLevel> map) =>
        NotEmpty(map)
            .Bind(WithinLength<MapLevel>(Some(1), Some(8)));
}
