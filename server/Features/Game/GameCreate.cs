namespace advisor.Features;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;
using Microsoft.Extensions.Logging;

public record GameCreateLocal(
    string Name,
    long EngineId,
    Stream Ruleset,
    List<MapLevel> Map,
    string Schedule,
    string TimeZone,
    DateTimeOffset? StartAt,
    DateTimeOffset? FinishAt
) : IRequest<Validation<Error, DbGame>>;


public class GameCreateHandler2 : IRequestHandler<GameCreateLocal, Validation<Error, DbGame>> {
    public GameCreateHandler2(Database database) {
        this.database = database;
    }

    private readonly Database database;

    // public Task<Validation<Error, DbGame>> Handle(GameCreateLocal request, CancellationToken cancellationToken) =>
    public void Handle(GameCreateLocal request, CancellationToken cancellationToken)
    {
        var rt = Runtime.New(database, cancellationToken);

        var temp =
        from rq in Validate(request)
        from game in GameInterpreter<Runtime>.Interpret(
                        from game in Mystcraft.Create(rq.Name, EngineId.New(rq.EngineId), rq.Ruleset, rq.Map, GameSchedule.New(rq.Schedule, rq.TimeZone, rq.StartAt, rq.FinishAt))
                        select game
                    )
        select game;
    }
    private static Validation<Error, string> ValidateName(string name) =>
        NotEmpty(name)
            .Bind(WithinLength(None, Some(Size.NAME)));

    // private static Task<Validation<Error, EngineId>> EngineExists(EngineId engineId) =>

    private static Validation<Error, List<MapLevel>> ValidateMap(List<MapLevel> map) =>
        NotEmpty(map)
            .Bind(WithinLength<MapLevel>(Some(1), Some(8)));

    private static Validation<Error, (Option<DateTimeOffset>, Option<DateTimeOffset>)> ValidatePeriod(Option<DateTimeOffset> start, Option<DateTimeOffset> finish) =>
        start.Match(
            Some: s => finish.Match(
                Some: f => s < f
                    ? Success<(Option<DateTimeOffset>, Option<DateTimeOffset>), Error>((s, f))
                    : Fail<(Option<DateTimeOffset>, Option<DateTimeOffset>), Error>(Error.New("Start date must be before finish date.")),
                None: () => Success<(Option<DateTimeOffset>, Option<DateTimeOffset>), Error>((s, None))
            ),
            None: () => finish.Match(
                Some: f => Success<(Option<DateTimeOffset>, Option<DateTimeOffset>), Error>((None, f)),
                None: () => Success<(Option<DateTimeOffset>, Option<DateTimeOffset>), Error>((None, None))
            )
        );


    private Validation<Error, GameCreateLocal> Validate(GameCreateLocal request) =>
        (ValidateName(request.Name), ValidateMap(request.Map), ValidatePeriod(Optional(request.StartAt), Optional(request.FinishAt)))
            .ApplyM((_, __, ___) => request);
}

public record GameCreateLocalResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public record GameCreateRemote(
    string Name,
    string ServerAddress,
    Stream Ruleset,
    List<MapLevel> Map,
    string Schedule,
    string TimeZone,
    DateTimeOffset? StartAt,
    DateTimeOffset? FinishAt
) : IRequest<GameCreateRemoteResult>;
public record GameCreateRemoteResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GameCreateHandler :
    IRequestHandler<GameCreateLocal, GameCreateLocalResult>,
    IRequestHandler<GameCreateRemote, GameCreateRemoteResult> {

    public GameCreateHandler(IAllGamesRepository gameRepo, IMediator mediator, ILogger<GameCreateHandler> logger) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.mediator = mediator;
        this.logger = logger;
    }

    private readonly IAllGamesRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMediator mediator;
    private readonly ILogger logger;

    public async Task<GameCreateLocalResult> Handle(GameCreateLocal request, CancellationToken cancellationToken)
        => await CreateGameAsync(
            DbGame.CreateLocal(
                name: request.Name,
                engineId: request.EngineId,
                ruleset: await request.Ruleset.ReadAllBytesAsync(cancellationToken),
                options: new GameOptions {
                    Map = request.Map,
                    Schedule = request.Schedule,
                    TimeZone = request.TimeZone,
                    StartAt = request.StartAt,
                    FinishAt = request.FinishAt
                }
            ),
            x => new GameCreateLocalResult(true, Game: x),
            error => new GameCreateLocalResult(false, Error: error.Message),
        cancellationToken);

    public async Task<GameCreateRemoteResult> Handle(GameCreateRemote request, CancellationToken cancellationToken)
        => await CreateGameAsync(
            DbGame.CreateRemote(
                name: request.Name,
                serverAddress: request.ServerAddress,
                ruleset: await request.Ruleset.ReadAllBytesAsync(cancellationToken),
                options: new GameOptions {
                    Map = request.Map,
                    Schedule = request.Schedule,
                    TimeZone = request.TimeZone,
                    StartAt = request.StartAt,
                    FinishAt = request.FinishAt
                }
            ),
            x => new GameCreateRemoteResult(true, Game: x),
            error => new GameCreateRemoteResult(false, Error: error.Message),
        cancellationToken);

    private Task<T> CreateGameAsync<T>(DbGame game, Func<DbGame, T> onSuccess, Func<Error, T> onFailure, CancellationToken cancellation)
        => unitOfWork.BeginTransaction(cancellation)
            .Bind(_ => gameRepo.Add(game))
            .Bind(g => unitOfWork.SaveChanges(cancellation)
                .Bind(() => mediator.Reconcile(g.Id, cancellation))
                .Return(g)
            )
            .RunWithRollback(unitOfWork, onSuccess, onFailure, cancellation);
}
