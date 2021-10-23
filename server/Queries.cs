namespace advisor {
    using System.Linq;
    using advisor.Persistence;

    public static class Queries {
        public static IQueryable<T> FilterByPlayer<T>(this IQueryable<T> query, long playerId) where T: InPlayerContext {
            return query.Where(x => x.PlayerId == playerId);
        }

        public static IQueryable<T> FilterByPlayer<T>(this IQueryable<T> query, InPlayerContext context) where T: InPlayerContext {
            return query.FilterByPlayer(context.PlayerId);
        }

        public static IQueryable<T> FilterByTurn<T>(this IQueryable<T> query, long playerId, int turnNumber) where T: InTurnContext {
            return query.FilterByPlayer(playerId).Where(x => x.TurnNumber == turnNumber);
        }

        public static IQueryable<T> FilterByTurn<T>(this IQueryable<T> query, InTurnContext context) where T: InTurnContext {
            return query.FilterByTurn(context.PlayerId, context.TurnNumber);
        }
    }
}
