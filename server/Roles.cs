namespace advisor
{
    using Microsoft.AspNetCore.Authorization;

    public static class Policies {
        public const string Root = "root";
        public const string GameMaster = "game-master";
        public const string UserManager = "user-manager";

        public static void AddPolicyAll(this AuthorizationOptions options, string name, params string[] roles) {
            options.AddPolicy(name, p => {
                foreach (var role in roles) {
                    p.RequireRole(role);
                }
            });
        }

        public static void AddPolicyAny(this AuthorizationOptions options, string name, params string[] roles) {
            options.AddPolicy(name, p => {
                p.RequireRole(roles);
            });
        }
    }
}
