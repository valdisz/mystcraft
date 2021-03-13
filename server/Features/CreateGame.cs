namespace advisor.Features
{
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record CreateGame(string Name) : IRequest<DbGame> {

    }

    public class CreateGameHandler : IRequestHandler<CreateGame, DbGame> {
        public CreateGameHandler(Database db)
        {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbGame> Handle(CreateGame request, CancellationToken cancellationToken) {
            var newGame = new DbGame  {
                Name = request.Name
            };

            await db.Games.AddAsync(newGame);
            await db.SaveChangesAsync();

            return newGame;
        }
    }
}
