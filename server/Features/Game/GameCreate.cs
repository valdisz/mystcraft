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

public record GameCreate(
    string Name,
    long GameEngineId,
    // Stream Ruleset,
    List<MapLevel> Map,
    string Schedule,
    string TimeZone,
    DateTimeOffset? StartAt,
    DateTimeOffset? FinishAt
) : IRequest<GameCreateResult>;

public record GameCreateResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error) {
    public static GameCreateResult New(Validation<Error, DbGame> result) =>
        result.Match(
            Succ: g => new GameCreateResult(true, Game: g),
            Fail: e => new GameCreateResult(false, Error: e.Head.Message)
        );
}

public class GameCreateHandler : IRequestHandler<GameCreate, GameCreateResult> {
    public GameCreateHandler(Database database) {
        this.database = database;
    }

    private readonly Database database;

    public Task<GameCreateResult> Handle(GameCreate request, CancellationToken cancellationToken) =>
        Validate(request)
            .Map(GameInterpreter<Runtime>.Interpret)
            .RunWrapped(Runtime.New(database, cancellationToken))
            .Map(GameCreateResult.New);

    private static Validation<Error, Mystcraft<DbGame>> Validate(GameCreate request) =>
    (
        ValidateName(request.Name).ForField("Name"),
        EngineId.New(request.GameEngineId).ForField(nameof(GameCreate.GameEngineId)),
        ValidateMap(request.Map).ForField("Map"),
        GameSchedule.New(request.Schedule, request.TimeZone).ForField("Schedule"),
        GamePeriod.New(Optional(request.StartAt), Optional(request.FinishAt)).ForField("Period")
    )
    .Apply(Mystcraft.CreateGame);

    private static Validation<Error, string> ValidateName(string name) =>
        NotEmpty(name)
            .Bind(WithinLength(1, Some(Size.NAME)));

    private static Validation<Error, Stream> ValidateRuleset(Stream ruleset) =>
        Optional(ruleset)
            .ToValidation(Error.New("Required."));

    private static Validation<Error, List<MapLevel>> ValidateMap(List<MapLevel> map) =>
        NotEmpty(map)
            .Bind(WithinLength<MapLevel>(Some(1), Some(8)));
}
