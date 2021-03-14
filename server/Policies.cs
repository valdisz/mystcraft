namespace advisor
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using advisor.Authorization;
    using Microsoft.AspNetCore.Authorization;

    public static class Roles {
        public const string Root = "root";
        public const string GameMaster = "game-master";
        public const string UserManager = "user-manager";
    }

    public static class Policies {
        public const string Root = "root";
        public const string GameMasters = "game-master";
        public const string UserManagers = "user-manager";
        public const string OwnPlayer = "own-player";

        public static void AddPolicyAllRoles(this AuthorizationOptions options, string name, params string[] roles) {
            options.AddPolicy(name, p => {
                foreach (var role in roles) {
                    p.RequireRole(role);
                }
            });
        }

        public static void AddPolicyAnyRole(this AuthorizationOptions options, string name, params string[] roles) {
            options.AddPolicy(name, p => {
                p.RequireRole(roles);
            });
        }

        public static void AddOwnPlayerPolicy(this AuthorizationOptions options) {
            options.AddPolicy(OwnPlayer, p => {
                p.RequireAuthenticatedUser();
                p.AddRequirements(new OwnPlayerRequirement());
            });
        }

        public static async Task<bool> AuthorizeOwnPlayer(this IAuthorizationService authorization, ClaimsPrincipal user, long playerId) {
            var result = await authorization.AuthorizeAsync(user, new GameContext(playerId), Policies.OwnPlayer);

            return result.Succeeded;
        }
    }
}
