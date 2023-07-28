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

public record GameCreateLocal(
    string Name,
    long GameEngineId,
    List<MapLevel> Levels,
    string Schedule,
    string TimeZone,
    DateTimeOffset? StartAt,
    DateTimeOffset? FinishAt,
    Stream GameIn,
    Stream PlayerIn
) : IRequest<GameCreateLocalResult>;


public record GameCreateLocalResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error) {
    public static GameCreateLocalResult New(Validation<Error, DbGame> result) =>
        result.Match(
            Succ: g => new GameCreateLocalResult(true, Game: g),
            Fail: e => new GameCreateLocalResult(false, Error: e.Head.Message)
        );
}

public class GameCreateLocalHandler : IRequestHandler<GameCreateLocal, GameCreateLocalResult> {
    public GameCreateLocalHandler(Database database) {
        this.database = database;
    }

    private readonly Database database;

    public Task<GameCreateLocalResult> Handle(GameCreateLocal request, CancellationToken cancellationToken) =>
        Validate(request)
            .Map(GameInterpreter<Runtime>.Interpret)
            .Unwrap(Runtime.New(database, cancellationToken))
            .Map(GameCreateLocalResult.New);

    private static Validation<Error, Mystcraft<DbGame>> Validate(GameCreateLocal request) =>
    (
        ValidateName(request.Name).ForField("Name"),
        GameEngineId.New(request.GameEngineId).ForField(nameof(GameCreateRemote.GameEngineId)),
        ValidateMap(request.Levels).ForField("Levels"),
        GameSchedule.New(request.Schedule, request.TimeZone).ForField("Schedule"),
        GamePeriod.New(Optional(request.StartAt), Optional(request.FinishAt)).ForField("Period"),
        ValidateStream(request.GameIn).ForField(nameof(GameCreateLocal.GameIn)),
        ValidateStream(request.PlayerIn).ForField(nameof(GameCreateLocal.PlayerIn))
    )
    .Apply(Mystcraft.CreateGameLocal);

    private static Validation<Error, string> ValidateName(string name) =>
        NotEmpty(name)
            .Bind(WithinLength(1, Some(Size.LABEL)));

    private static Validation<Error, byte[]> ValidateStream(Stream stream) =>
        Optional(stream)
            .ToValidation(Error.New("Required."))
            .Map(s => s.ReadAllBytes());

    private static Validation<Error, List<MapLevel>> ValidateMap(List<MapLevel> map) =>
        NotEmpty(map)
            .Bind(WithinLength<MapLevel>(Some(1), Some(8)));
}
