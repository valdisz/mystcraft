namespace advisor.Hanfgire;

using System.Linq;
using Hangfire.Annotations;
using Hangfire.Dashboard;

public class RoleBasedDashboardAuthorizationFilter : IDashboardAuthorizationFilter {
    public RoleBasedDashboardAuthorizationFilter(params string[] roles) {
        this.roles = roles ?? new string[0];
    }

    private readonly string[] roles;

    public bool Authorize([NotNull] DashboardContext context) {
        var httpContext = context.GetHttpContext();

        if (!httpContext.User.Identity.IsAuthenticated) return false;
        if (!roles.All(role => httpContext.User.IsInRole(role))) return false;

        return true;
    }
}
