namespace advisor.Persistence;

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class UserInterceptor : SaveChangesInterceptor {
    public UserInterceptor(IUserAccessor userAccessor) {
        this.userAccessor = userAccessor;
    }

    private readonly IUserAccessor userAccessor;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result) {
        if (!userAccessor.Principal.Identity.IsAuthenticated) {
            return result;
        }

        if (!long.TryParse(userAccessor.Principal.FindFirstValue(WellKnownClaimTypes.USER_ID) ?? "", out var userId)) {
            return result;
        }

        var ct = eventData.Context.ChangeTracker;

        foreach (var entry in ct.Entries<WithCreator>()) {
            if (entry.State != EntityState.Added) {
                continue;
            }

            entry.Entity.CreatedByUserId = userId;
        }

        foreach (var entry in ct.Entries<WithUpdater>()) {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified) {
                continue;
            }

            entry.Entity.UpdatedByUserId = userId;
        }

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) {
        return new ValueTask<InterceptionResult<int>>(SavingChanges(eventData, result));
    }
}

