namespace advisor
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using advisor.Authorization;
    using HotChocolate;
    using HotChocolate.Resolvers;
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

        public static void AddOwnPlayerPolicy(this AuthorizationOptions options, string name) {
            options.AddPolicy(name, p => {
                p.RequireAuthenticatedUser();
                p.AddRequirements(new OwnPlayerRequirement());
            });
        }

        public static async Task<bool> AuthorizeOwnPlayerAsync(this IAuthorizationService authorization, ClaimsPrincipal user, IResolverContext context) {
            var result = await authorization.AuthorizeAsync(user, context, Policies.OwnPlayer);

            return result.Succeeded;
        }
    }

    public static class ResolverAuthorizationExtensions {
        public static async Task<bool> AuthorizeAsync(this IResolverContext resolver, string policyName) {
            var auth = resolver.Service<IAuthorizationService>();
            var result = await auth.AuthorizeAsync(resolver.GetUser(), resolver, policyName);

            if (result.Succeeded) {
                return true;
            }

            var failure = result.Failure;

            if (failure.FailureReasons.Any()) {
                foreach (var reson in failure.FailureReasons) {
                    resolver.ReportError(ErrorBuilder.New()
                        .SetMessage(reson.Message)
                        .SetCode(ErrorCodes.Authentication.NotAuthorized)
                        .SetPath(resolver.Path)
                        .AddLocation(resolver.Selection.SyntaxNode)
                        .Build()
                    );
                }
            }
            else {
                resolver.ReportError(ErrorBuilder.New()
                    .SetMessage("The current user is not authorized to access this resource.")
                    .SetCode(ErrorCodes.Authentication.NotAuthorized)
                    .SetPath(resolver.Path)
                    .AddLocation(resolver.Selection.SyntaxNode)
                    .Build()
                );
            }

            return false;
        }
    }
}
