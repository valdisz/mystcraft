namespace advisor.Persistence;

using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Effects.Traits;
using Microsoft.EntityFrameworkCore;

public interface HasDatabase<RT>: HasCancel<RT>
    where RT : struct, HasDatabase<RT>, HasCancel<RT> {

    Eff<RT, DatabaseIO> DatabaseEff { get; }
}

public interface DatabaseIO {
    /// <summary>
    /// List of registered users who can access the system depending on their roles
    /// </summary>
    DbSet<DbUser> Users { get; }

    /// <summary>
    /// List of game engines which are supported by the system
    /// </summary>
    DbSet<DbGameEngine> GameEngines { get; }

    /// <summary>
    /// List of local and remote games which are registered in the system
    /// </summary>
    DbSet<DbGame> Games { get; }


    /////////////////////////////////////////////
    ///// Game turn data as the engine returns it

    /// <summary>
    /// List of turns for the game
    /// </summary>
    DbSet<DbTurn> Turns { get; }

    /// <summary>
    /// List of reports for the game
    /// </summary>
    DbSet<DbReport> Reports { get; }

    /// <summary>
    /// List of articles for the game written by player of by the game engine itself
    /// </summary>
    DbSet<DbArticle> Articles{ get; }


    /////////////////////////////////////////////
    ///// Player controled factions and data which is modified during the game

    /// <summary>
    /// List of pending players for the game
    /// </summary>
    DbSet<DbRegistration> Registrations { get; }

    /// <summary>
    /// List of players for the game
    /// </summary>
    DbSet<DbPlayer> Players { get; }

    /// <summary>
    /// List of player turns for the game
    /// </summary>
    DbSet<DbPlayerTurn> PlayerTurns { get; }

    /// <summary>
    /// List of additional reports that playes added additionally for the game
    /// </summary>
    DbSet<DbAdditionalReport> AditionalReports { get; }

    /// <summary>
    /// List of orders that players submitted for the game
    /// </summary>
    DbSet<DbOrders> Orders { get; }


    /////////////////////////////////////////////
    ///// Parsed and normalized game state for the each player based on their reports

    /// <summary>
    /// List of factions for the game turn as seen by the player
    /// </summary>
    DbSet<DbFaction> Factions { get; }

    DbSet<DbAttitude> Attitudes { get; }
    DbSet<DbEvent> Events { get; }
    DbSet<DbRegion> Regions { get; }
    DbSet<DbProductionItem> Production { get; }
    DbSet<DbTradableItem> Markets { get; }
    DbSet<DbExit> Exits { get; }
    DbSet<DbStructure> Structures { get; }
    DbSet<DbUnit> Units { get; }
    DbSet<DbUnitItem> Items { get; }
    DbSet<DbBattle> Battles { get; }

    // Additional features not provided by the game engine
    DbSet<DbAlliance> Alliances { get; }
    DbSet<DbAllianceMember> AllianceMembers { get; }
    DbSet<DbStudyPlan> StudyPlans { get; }
    DbSet<DbTurnStatisticsItem> TurnStatistics { get; }
    DbSet<DbRegionStatisticsItem> RegionStatistics { get; }
    DbSet<DbTreasuryItem> Treasury { get; }

    ValueTask<DbGame> Add(DbGame game, CancellationToken ct);
}
