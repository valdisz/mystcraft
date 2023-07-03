namespace advisor.Schema;

using System.Linq;
using System.Threading.Tasks;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using MediatR;
using advisor.Features;
using advisor.Persistence;

public class Query {
    [Authorize(Policy = Policies.GameMasters)]
    [UseOffsetPaging]
    public IQueryable<DbGameEngine> GameEngines(Database db) => db.GameEngines;

    [UseOffsetPaging]
    public async ValueTask<IQueryable<DbGame>> Games(IResolverContext context, Database db) =>
        (await GameInterpreter<Runtime>.Interpret(
            from games in Mystcraft.ReadManyGames()
            select games.OrderByDescending(g => g.CreatedAt)
        ).Run(Runtime.New(db, context.RequestAborted)))
        .Match(
            Succ: games => games,
            Fail: ex => throw ex.ToException()
        );

    [Authorize(Policy = Policies.UserManagers)]
    [UseOffsetPaging]
    public IQueryable<DbUser> Users(Database db) => db.Users;

    [Authorize]
    public ValueTask<DbUser> Me(Database db, [GlobalState] long currentUserId) => db.Users.FindAsync(currentUserId);

    [Authorize]
    public Task<BackgroundJob> Job(IMediator mediator, string jobId) {
        return mediator.Send(new GetJobStatus(jobId));
    }
}
