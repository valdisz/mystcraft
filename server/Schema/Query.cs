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
using advisor.Model;
using HotChocolate.Data;

public class Query {
    [Authorize(Policy = Policies.GameMasters)]
    [UseOffsetPaging]
    [UseProjection]
    public ValueTask<IOrderedQueryable<DbGameEngine>> GameEngines(IResolverContext context, Database db) =>
        GameInterpreter<Runtime>.Interpret(
            from items in Mystcraft.ReadManyGameEngines()
            select items
        )
        .Run(Runtime.New(db, context.RequestAborted))
        .Unwrap();

    [UseOffsetPaging]
    [UseProjection]
    public ValueTask<IOrderedQueryable<DbGame>> Games(IResolverContext context, Database db) =>
        GameInterpreter<Runtime>.Interpret(
            from items in Mystcraft.ReadManyGames()
            select items
        )
        .Run(Runtime.New(db, context.RequestAborted))
        .Unwrap();

    [Authorize(Policy = Policies.UserManagers)]
    [UseOffsetPaging]
    [UseProjection]
    public IQueryable<DbUser> Users(Database db) =>
        db.Users.OrderByDescending(x => x.CreatedAt);

    [Authorize]
    public ValueTask<DbUser> Me(Database db, [GlobalState] long currentUserId) =>
        db.Users.FindAsync(currentUserId);

    [Authorize]
    public Task<BackgroundJob> Job(IMediator mediator, string jobId) =>
        mediator.Send(new GetJobStatus(jobId));
}
