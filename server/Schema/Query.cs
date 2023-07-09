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
    public IQueryable<DbGameEngine> GameEngines(Database db) =>
        db.GameEngines.OrderByDescending(x => x.CreatedAt);

    [UseOffsetPaging]
    public async ValueTask<IQueryable<DbGame>> Games(IResolverContext context, Database db) {
        var effect = GameInterpreter<Runtime>.Interpret(
            from games in Mystcraft.ReadManyGames()
            select games.OrderByDescending(g => g.CreatedAt)
        );

        var result = await effect.Run(Runtime.New(db, context.RequestAborted));

        var value = result.Match(
            Succ: games => games,
            Fail: ex => throw ex.ToException()
        );

        return value;
    }

    [Authorize(Policy = Policies.UserManagers)]
    [UseOffsetPaging]
    public IQueryable<DbUser> Users(Database db) =>
        db.Users.OrderByDescending(x => x.CreatedAt);

    [Authorize]
    public ValueTask<DbUser> Me(Database db, [GlobalState] long currentUserId) =>
        db.Users.FindAsync(currentUserId);

    [Authorize]
    public Task<BackgroundJob> Job(IMediator mediator, string jobId) =>
        mediator.Send(new GetJobStatus(jobId));
}
