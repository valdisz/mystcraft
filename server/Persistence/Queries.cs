namespace advisor.Persistence;

using System.Linq;

public static class Queries {
    public static IQueryable<T> OnlyPlayer<T>(this IQueryable<T> query, long playerId) where T: InPlayerContext
        => query.Where(x => x.PlayerId == playerId);

    public static IQueryable<T> OnlyPlayer<T>(this IQueryable<T> query, InPlayerContext context) where T: InPlayerContext
        => query.OnlyPlayer(context.PlayerId);

    public static IQueryable<T> OnlyPlayer<T>(this IQueryable<T> query, DbPlayer context) where T: InPlayerContext
        => query.OnlyPlayer(context.Id);

    public static IQueryable<T> InTurn<T>(this IQueryable<T> query, long playerId, int turnNumber) where T: InTurnContext
        => query.OnlyPlayer(playerId).Where(x => x.TurnNumber == turnNumber);

    public static IQueryable<T> InTurn<T>(this IQueryable<T> query, InTurnContext context) where T: InTurnContext
        => query.InTurn(context.PlayerId, context.TurnNumber);

    public static IQueryable<T> InTurn<T>(this IQueryable<T> query, DbPlayerTurn context) where T: InTurnContext
        => query.InTurn(context.PlayerId, context.TurnNumber);

    public static IQueryable<T> OnlyFaction<T>(this IQueryable<T> query, long playerId, int turnNumber, int factionNumber) where T: InFactionContext
        => query.InTurn(playerId, turnNumber).Where(x => x.FactionNumber == factionNumber);

    public static IQueryable<T> OnlyFaction<T>(this IQueryable<T> query, InFactionContext context) where T: InFactionContext
        => query.OnlyFaction(context.PlayerId, context.TurnNumber, context.FactionNumber);

    public static IQueryable<T> OnlyFaction<T>(this IQueryable<T> query, DbFaction context) where T: InFactionContext
        => query.OnlyFaction(context.PlayerId, context.TurnNumber, context.Number);

    public static IQueryable<DbPlayer> OnlyActivePlayers(this IQueryable<DbPlayer> query)
        => query.Where(x => !x.IsQuit);

    public static IQueryable<T> InGame<T>(this IQueryable<T> query, long gameId) where T: InGameContext
        => query.Where(x => x.GameId == gameId);

    public static IQueryable<T> InGame<T>(this IQueryable<T> query, DbGame game) where T: InGameContext
        => query.InGame(game.Id);
}
