namespace advisor.Authorization {
    using System;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;

    public class OwnPlayerRequirement : IAuthorizationRequirement {

    }

    public class OwnPlayerAuthorizationHandler : AuthorizationHandler<OwnPlayerRequirement, GameContext> {
        public OwnPlayerAuthorizationHandler(Database db, IMemoryCache cache) {
            this.db = db;
            this.cache = cache;
        }

        private readonly Database db;
        private readonly IMemoryCache cache;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnPlayerRequirement requirement, GameContext resource) {
            var key = $"{resource.PlayerId}-PlayerUserId";
            var playerUserId = await cache.GetOrCreateAsync(key, async entry => {
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(15));
                var player = await db.Players.AsNoTracking().FirstOrDefaultAsync(x => x.Id == resource.PlayerId);

                return player?.UserId;
            });

            var userId = context.User.FindFirst(WellKnownClaimTypes.UserId)?.Value;
            if (userId == null) {
                context.Fail();
                return;
            }

            if (long.Parse(userId) == playerUserId) {
                context.Succeed(requirement);
            }
            else {
                context.Fail();
            }
        }
    }
}
