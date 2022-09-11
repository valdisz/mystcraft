namespace advisor.Schema {
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class UserType : ObjectType<DbUser> {
        protected override void Configure(IObjectTypeDescriptor<DbUser> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.Id)
                .ResolveNode(async (ctx, id) => {
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
                        .AsNoTracking()
                        .SingleOrDefaultAsync(x => x.Id == id);
                });

            descriptor.Authorize();
        }
    }

    [ExtendObjectType("User")]
    public class UserResolvers {
        public Task<List<DbPlayer>> Players(Database db, [Parent] DbUser user) {
            return db.Players
                .AsNoTracking()
                .Where(x => x.UserId == user.Id)
                .ToListAsync();
        }
    }
}
