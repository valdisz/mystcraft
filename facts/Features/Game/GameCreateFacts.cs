// namespace advisor.Features.Game;

// using advisor.Features;
// using advisor.Persistence;
// using MediatR;
// using Microsoft.Extensions.Logging;

// public class GameCreateHandlerFacts {
//     public GameCreateHandlerFacts() {
//         this.gameRepo = new Mock<IAllGamesRepository>();
//         this.unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
//         this.mediator = new Mock<IMediator>();
//         this.logger = new Mock<ILogger<GameCreateHandler>>();

//         this.gameRepo.SetupGet(x => x.UnitOfWork).Returns(this.unitOfWork.Object);

//         this.handler = new GameCreateHandler(gameRepo.Object, mediator.Object, logger.Object);
//     }

//     private readonly Mock<IAllGamesRepository> gameRepo;
//     private readonly Mock<IUnitOfWork> unitOfWork;
//     private readonly Mock<IMediator> mediator;
//     private readonly Mock<ILogger<GameCreateHandler>> logger;

//     private readonly GameCreateHandler handler;

//     [Fact]
//     public async Task Can_handle_create_game_local() {
//         var itIsAnyCancellation = It.IsAny<CancellationToken>();

//         var seq = new MockSequence();
//         unitOfWork.InSequence(seq).Setup(x => x.BeginTransaction(itIsAnyCancellation)).Returns(Task.CompletedTask);
//         unitOfWork.InSequence(seq).Setup(x => x.SaveChanges(itIsAnyCancellation)).Returns(Task.FromResult(1));
//         unitOfWork.InSequence(seq).Setup(x => x.CommitTransaction(itIsAnyCancellation)).Returns(Task.FromResult(true));

//         var input = new GameCreate(
//             Name: "test",
//             EngineId: 1,
//             Ruleset: Stream.Null,
//             Map: new List<Persistence.MapLevel> {
//                 new (Level: 0, Label: "nexus", Width: 1, Height: 1)
//             },
//             Schedule: "* * * * *",
//             TimeZone: "Etc/UTC",
//             StartAt: null,
//             FinishAt: null
//         );

//         var result = await handler.Handle(input, default);

//         gameRepo.Verify(x => x.Add(It.IsAny<DbGame>()), Times.Once());

//         mediator.Verify(x => x.Send(It.IsAny<Reconcile>(), itIsAnyCancellation), Times.Once());

//         unitOfWork.Verify(x => x.BeginTransaction(itIsAnyCancellation), Times.Once());
//         unitOfWork.Verify(x => x.SaveChanges(itIsAnyCancellation), Times.Once());
//         unitOfWork.Verify(x => x.CommitTransaction(itIsAnyCancellation), Times.Once());

//         result.IsSuccess.Should().BeTrue();
//         result.Error.Should().BeNull();
//         result.Game.Should().NotBeNull();

//         result.Game.Name.Should().Be(input.Name);
//     }

//     [Fact]
//     public async Task Return_error_if_cannot_commit_transaction() {
//         var itIsAnyCancellation = It.IsAny<CancellationToken>();

//         var seq = new MockSequence();
//         unitOfWork.InSequence(seq).Setup(x => x.BeginTransaction(itIsAnyCancellation)).Returns(Task.CompletedTask);
//         unitOfWork.InSequence(seq).Setup(x => x.SaveChanges(itIsAnyCancellation)).Returns(Task.FromResult(1));
//         unitOfWork.InSequence(seq).Setup(x => x.CommitTransaction(itIsAnyCancellation)).Returns(Task.FromResult(false));

//         var input = new GameCreate(
//             Name: "",
//             EngineId: 1,
//             Ruleset: Stream.Null,
//             Map: new List<Persistence.MapLevel> {
//                 new (Level: 0, Label: "nexus", Width: 1, Height: 1)
//             },
//             Schedule: "* * * * *",
//             TimeZone: "Etc/UTC",
//             StartAt: null,
//             FinishAt: null
//         );

//         var result = await handler.Handle(input, default);

//         gameRepo.Verify(x => x.Add(It.IsAny<DbGame>()), Times.Once());

//         mediator.Verify(x => x.Send(It.IsAny<Reconcile>(), itIsAnyCancellation), Times.Once());

//         unitOfWork.Verify(x => x.BeginTransaction(itIsAnyCancellation), Times.Once());
//         unitOfWork.Verify(x => x.SaveChanges(itIsAnyCancellation), Times.Once());
//         unitOfWork.Verify(x => x.CommitTransaction(itIsAnyCancellation), Times.Once());

//         result.IsSuccess.Should().BeFalse();
//         result.Error.Should().NotBeNull();
//         result.Game.Should().BeNull();
//     }

//     [Fact]
//     public async Task Return_error_if_cannot_save_changes() {
//         var itIsAnyCancellation = It.IsAny<CancellationToken>();

//         var seq = new MockSequence();
//         unitOfWork.InSequence(seq).Setup(x => x.BeginTransaction(itIsAnyCancellation)).Returns(Task.CompletedTask);
//         unitOfWork.InSequence(seq).Setup(x => x.SaveChanges(itIsAnyCancellation)).Throws<Exception>();
//         unitOfWork.InSequence(seq).Setup(x => x.RollbackTransaction(itIsAnyCancellation)).Returns(Task.FromResult(true));

//         var input = new GameCreate(
//             Name: "",
//             EngineId: 1,
//             Ruleset: Stream.Null,
//             Map: new List<Persistence.MapLevel> {
//                 new (Level: 0, Label: "nexus", Width: 1, Height: 1)
//             },
//             Schedule: "* * * * *",
//             TimeZone: "Etc/UTC",
//             StartAt: null,
//             FinishAt: null
//         );

//         var result = await handler.Handle(input, default);

//         gameRepo.Verify(x => x.Add(It.IsAny<DbGame>()), Times.Once());

//         mediator.Verify(x => x.Send(It.IsAny<Reconcile>(), itIsAnyCancellation), Times.Never());

//         unitOfWork.Verify(x => x.BeginTransaction(itIsAnyCancellation), Times.Once());
//         unitOfWork.Verify(x => x.SaveChanges(itIsAnyCancellation), Times.Once());
//         unitOfWork.Verify(x => x.RollbackTransaction(itIsAnyCancellation), Times.Once());
//         unitOfWork.Verify(x => x.CommitTransaction(itIsAnyCancellation), Times.Never());

//         result.IsSuccess.Should().BeFalse();
//         result.Error.Should().NotBeNull();
//         result.Game.Should().BeNull();
//     }
// }
