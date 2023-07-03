// FIXME
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

// public class GameJoinRemoteHandler : IRequestHandler<GameJoinRemote, GameJoinRemoteResult> {
//     public GameJoinRemoteHandler(IAllGamesRepository gameRepo, IPlayerRepository playerRepo, IHttpClientFactory httpFactory, IMediator mediator) {
//         this.gameRepo = gameRepo;
//         this.playerRepo = playerRepo;
//         this.unitOfWork = gameRepo.UnitOfWork;
//         this.httpFactory = httpFactory;
//         this.mediator = mediator;
//     }

//     private readonly IAllGamesRepository gameRepo;
//     private readonly IPlayerRepository playerRepo;
//     private readonly IUnitOfWork unitOfWork;
//     private readonly IHttpClientFactory httpFactory;
//     private readonly IMediator mediator;

//     public Task<GameJoinRemoteResult> Handle(GameJoinRemote request, CancellationToken cancellationToken)
//         => unitOfWork.BeginTransaction(cancellationToken)
//             .Bind(_ => gameRepo.GetOneGame(request.GameId, withTracking: false, cancellation: cancellationToken))
//             .Validate(game => game switch {
//                 { Type: Persistence.GameType.LOCAL } => Failure<DbGame>("Cannot claim players in local game."),
//                 { Status: GameStatus.NEW } => Failure<DbGame>("Game not yet started."),
//                 { Status: GameStatus.LOCKED } => Failure<DbGame>("Game is processing a turn."),
//                 { Status: GameStatus.STOPED }  => Failure<DbGame>("Game compleated."),
//                 _ => Success(game)
//             })
//             .Select(game => (game: game, repo: playerRepo.Specialize(game)))
//             .Bind(state => state.repo.GetOnePlayer(request.PlayerId, cancellation: cancellationToken)
//                 .Bind(maybePlayer => maybePlayer
//                     .Select(player => player switch {
//                         { IsClaimed: true } => Failure<DbPlayer>($"Player already claimed by {(player.UserId == request.UserId ? "this" : "another" )} player."),
//                         _ => Success(player)
//                     })
//                     .Unwrap(() => Failure<DbPlayer>("Player does not exist."))
//                 )
//                 .Bind(player => DoesNotContainActivePlayer(state.repo, request.UserId, cancellationToken)
//                     .Bind(() => DownloadRemoteReport(state.game.Options.ServerAddress, player.Number, request.Password, cancellationToken)
//                         .Bind(report => UploadPlayerReport(player.Id, report, cancellationToken))
//                     )
//                     .Return(player)
//                 )
//                 .Do(player => player.UserId = request.UserId)
//                 .Bind(state.repo.Update)
//             )
//             .RunWithRollback(
//                 unitOfWork,
//                 player => new GameJoinRemoteResult(true, Player: player),
//                 error => new GameJoinRemoteResult(false, error.Message),
//                 cancellationToken
//             );

//     private AsyncIO<advisor.Unit> DoesNotContainActivePlayer(ISpecializedPlayerRepository repo, long userId, CancellationToken cancellation)
//         => async () => await repo.Players
//             .AsNoTracking()
//             .OnlyActivePlayers()
//             .AnyAsync(x => x.UserId == userId, cancellation)
//                 ? Failure<advisor.Unit>("Game already includes active player from this user.")
//                 : Success(unit);

//     private AsyncIO<advisor.Unit> UploadPlayerReport(long playerId, string report, CancellationToken cancellation) {
//         throw new NotImplementedException();
//     }

//     private AsyncIO<string> DownloadRemoteReport(string serverAddress, int number, string password, CancellationToken cancellation)
//         => async () => {
//             var remote = new NewOrigins(serverAddress, httpFactory);
//             try {
//                 return Success(await remote.DownloadReportAsync(number, password, cancellation));
//             }
//             catch (WrongFactionOrPasswordException ex) {
//                 return Failure<string>("Wrong faction number or password.", ex);
//             }
//         };
// }
