namespace advisor.Persistence;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class TimeInterceptor : SaveChangesInterceptor {
    public TimeInterceptor(ITime time)
    {
        this.time = time;
    }

    private readonly ITime time;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result) {
        var ct = eventData.Context.ChangeTracker;

        var now = time.UtcNow;

        foreach (var entry in ct.Entries<WithCreationTime>()) {
            if (entry.State != EntityState.Added) {
                continue;
            }

            entry.Entity.CreatedAt = now;
        }

        foreach (var entry in ct.Entries<WithUpdateTime>()) {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified) {
                continue;
            }

            entry.Entity.UpdatedAt = now;
        }

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) {
        return new ValueTask<InterceptionResult<int>>(SavingChanges(eventData, result));
    }
}

