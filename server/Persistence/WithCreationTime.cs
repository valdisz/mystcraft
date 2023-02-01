namespace advisor.Persistence;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

public interface WithCreationTime {
    DateTimeOffset CreatedAt { get; set; }
}

public class CreationTimeInterceptor : SaveChangesInterceptor {
    public CreationTimeInterceptor(ITime time)
    {
        this.time = time;
    }

    private readonly ITime time;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result) {
        var ct = eventData.Context.ChangeTracker;

        var now = time.UtcNow;
        foreach (var entry in ct.Entries<WithCreationTime>()) {
            entry.Entity.CreatedAt = now;
        }

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) {
        return new ValueTask<InterceptionResult<int>>(SavingChanges(eventData, result));
    }
}

