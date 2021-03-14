namespace advisor.Authorization {
    using System.Threading.Tasks;
    using advisor.Persistence;
    using Microsoft.AspNetCore.Authorization;

    public class OwnPlayerRequirement : IAuthorizationRequirement {

    }

    public record GameContext(long PlayerId);

    public class OwnPlayerAuthorizationHandler : AuthorizationHandler<OwnPlayerRequirement, GameContext> {
        public OwnPlayerAuthorizationHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnPlayerRequirement requirement, GameContext resource) {
            var player = await db.Players.FindAsync(resource.PlayerId);

            var userId = context.User.FindFirst(WellKnownClaimTypes.UserId)?.Value;
            if (userId == null) {
                context.Fail();
                return;
            }

            if (long.Parse(userId) == player.UserId) {
                context.Succeed(requirement);
            }
        }
    }
}
