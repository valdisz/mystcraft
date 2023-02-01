namespace advisor;

using System.Linq;
using Microsoft.EntityFrameworkCore;

public static class EFQueryableExtensions {
    public static IQueryable<T> WithTracking<T>(this IQueryable<T> input, bool enabled) where T: class => enabled ? input : input.AsNoTracking();
}
