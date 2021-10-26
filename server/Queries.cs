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

        public static IQueryable<T> FilterByPlayer<T>(this IQueryable<T> query, DbPlayer context) where T: InPlayerContext {
            return query.FilterByPlayer(context.Id);
        }

        public static IQueryable<T> FilterByTurn<T>(this IQueryable<T> query, long playerId, int turnNumber) where T: InTurnContext {
            return query.FilterByPlayer(playerId).Where(x => x.TurnNumber == turnNumber);
        }

        public static IQueryable<T> FilterByTurn<T>(this IQueryable<T> query, InTurnContext context) where T: InTurnContext {
            return query.FilterByTurn(context.PlayerId, context.TurnNumber);
        }

        public static IQueryable<T> FilterByTurn<T>(this IQueryable<T> query, DbTurn context) where T: InTurnContext {
            return query.FilterByTurn(context.PlayerId, context.Number);
        }

        public static IQueryable<T> FilterByFaction<T>(this IQueryable<T> query, long playerId, int turnNumber, int factionNumber) where T: InFactionContext {
            return query.FilterByTurn(playerId, turnNumber).Where(x => x.FactionNumber == factionNumber);
        }

        public static IQueryable<T> FilterByFaction<T>(this IQueryable<T> query, InFactionContext context) where T: InFactionContext {
            return query.FilterByFaction(context.PlayerId, context.TurnNumber, context.FactionNumber);
        }

        public static IQueryable<T> FilterByFaction<T>(this IQueryable<T> query, DbFaction context) where T: InFactionContext {
            return query.FilterByFaction(context.PlayerId, context.TurnNumber, context.Number);
        }
    }
}
