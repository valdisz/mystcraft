namespace advisor;

using System.Linq;
using System.Threading.Tasks;
using advisor.Persistence;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate;
using HotChocolate.Types;

public class Query {
    [Authorize(Policy = Policies.GameMasters)]
    [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 100)]
    public IQueryable<DbGameEngine> GameEngines(Database db) => db.GameEngines;

    [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 100)]
    public IQueryable<DbGame> Games(Database db) => db.Games;

    [Authorize(Policy = Policies.UserManagers)]
    [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 100)]
    public IQueryable<DbUser> Users(Database db) => db.Users;

    [Authorize]
    public ValueTask<DbUser> Me(Database db, [GlobalState] long currentUserId) => db.Users.FindAsync(currentUserId);
}
