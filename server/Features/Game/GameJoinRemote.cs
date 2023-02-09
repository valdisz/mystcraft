namespace advisor.Features;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Remote;
using advisor.Schema;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GameJoinRemote(long UserId, long GameId, long PlayerId, string Password) : IRequest<GameJoinRemoteResult>;

public record GameJoinRemoteResult(bool IsSuccess, string Error = null, DbPlayer Player = null) : MutationResult(IsSuccess, Error);

public class GameJoinRemoteHandler : IRequestHandler<GameJoinRemote, GameJoinRemoteResult> {
    public GameJoinRemoteHandler(IGameRepository gameRepo, IHttpClientFactory httpFactory, IMediator mediator) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.httpFactory = httpFactory;
        this.mediator = mediator;
    }

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IHttpClientFactory httpFactory;
    private readonly IMediator mediator;

    public Task<GameJoinRemoteResult> Handle(GameJoinRemote request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(_ => gameRepo.GetOneGame(request.GameId, withTracking: false, cancellation: cancellationToken))
            .Select(maybeGame => maybeGame
                .Select(game => game switch {
                    { Type: Persistence.GameType.LOCAL } => Failure<DbGame>("Cannot claim players in local game."),
                    { Status: GameStatus.NEW } => Failure<DbGame>("Game not yet started."),
                    { Status: GameStatus.LOCKED } => Failure<DbGame>("Game is processing a turn."),
                    { Status: GameStatus.COMPLEATED }  => Failure<DbGame>("Game compleated."),
                    _ => Success(game)
                })
                .Unwrap(() => Failure<DbGame>("Game does not exist."))
            )
            .Bind(game => gameRepo.GetOnePlayer(game, request.PlayerId, cancellation: cancellationToken)
                .Select(maybePlayer => maybePlayer
                    .Select(player => player switch {
                        { IsClaimed: true } => Failure<DbPlayer>($"Player already claimed by {(player.UserId == request.UserId ? "this" : "another" )} player."),
                        _ => Success(player)
                    })
                    .Unwrap(() => Failure<DbPlayer>("Player does not exist."))
                )
                .Bind(player => DoesNotContainActivePlayer(game, request.UserId, cancellationToken)
                    .Bind(_ => DownloadRemoteReport(game.Options.ServerAddress, player.Number, request.Password, cancellationToken))
                    .Bind(report => UploadPlayerReport(player.Id, report, cancellationToken))
                    .Bind(_ => {
                        player.UserId = request.UserId;
                        gameRepo.Update(player);

                        return unitOfWork.CommitTransaction(cancellationToken);
                    })
                    .Select(_ => new GameJoinRemoteResult(true, Player: player))
                )
            )
            .OnFailure(_ => unitOfWork.RollbackTransaction(cancellationToken))
            .Run()
            .Unwrap(error => new GameJoinRemoteResult(false, error.Message));

    private AsyncIO<advisor.Unit> DoesNotContainActivePlayer(DbGame game, long userId, CancellationToken cancellation)
        => async () => await gameRepo.QueryPlayers(game)
            .AsNoTracking()
            .OnlyActivePlayers()
            .AnyAsync(x => x.UserId == userId, cancellation)
                ? Failure<advisor.Unit>("Game already includes active player from this user.")
                : Success(unit);

    private AsyncIO<advisor.Unit> UploadPlayerReport(long playerId, string report, CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }

    private AsyncIO<string> DownloadRemoteReport(string serverAddress, int number, string password, CancellationToken cancellation)
        => async () => {
            var remote = new NewOrigins(serverAddress, httpFactory);
            try {
                return Success(await remote.DownloadReportAsync(number, password, cancellation));
            }
            catch (WrongFactionOrPasswordException ex) {
                return Failure<string>("Wrong faction number or password.", ex);
            }
        };
}
