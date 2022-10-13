namespace advisor.Schema;

using System.Linq;
using System.Threading.Tasks;
using advisor.Persistence;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate;
using HotChocolate.Types;
using advisor.Features;
using MediatR;
using HotChocolate.Types.Relay;

public class Query {
    [Authorize(Policy = Policies.GameMasters)]
    [UseOffsetPaging]
    public IQueryable<DbGameEngine> GameEngines(Database db) => db.GameEngines;

    [UseOffsetPaging]
    public IQueryable<DbGame> Games(Database db) => db.Games;

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
