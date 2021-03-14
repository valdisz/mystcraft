namespace advisor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class UserType : ObjectType<DbUser> {
        protected override void Configure(IObjectTypeDescriptor<DbUser> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver(async (ctx, id) => {
                    var currentUserId = (long) ctx.ContextData["currentUserId"];

                    if (id != currentUserId) {
                        var authorization = ctx.Service<IAuthorizationService>();
                        var principal = ctx.ContextData["ClaimsPrincipal"] as ClaimsPrincipal;
                        var result = await authorization.AuthorizeAsync(principal, ctx, Policies.UserManagers);
                        if (!result.Succeeded) {
                            ctx.ReportError(ErrorBuilder.New()
                                .SetCode(ErrorCodes.Authentication.NotAuthorized)
                                .Build()
                            );
                            return null;
                        }
                    }

                    var db = ctx.Service<Database>();
                    return await db.Users
                        .SingleOrDefaultAsync(x => x.Id == id);
                });

            descriptor.Authorize();
        }
    }

    [ExtendObjectType(Name = "User")]
    public class UserResolvers {
        public UserResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbPlayer>> Players([Parent] DbUser user) {
            return db.Players
                .Include(x => x.Game)
                .Include(x => x.UniversityMembership)
                .ThenInclude(x => x.University)
                .Where(x => x.UserId == user.Id)
                .ToListAsync();
        }
    }
}
