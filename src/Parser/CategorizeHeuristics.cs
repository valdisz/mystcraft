namespace atlantis
{
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public static class CategorizeHeuristics {
        static readonly Regex battle = new Regex(@"^.+attacks.+!$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex factionStausItem = new Regex(@"^.+\:.+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool MatchReportStart(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Atlantis Report For");

        public static bool MatchFactionStatus(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Faction Status:");

        public static bool MatchFactionStatusItem(this Cursor<MultiLineBlock> cursor)
            => factionStausItem.IsMatch(cursor.Value.Text);

        public static bool MatchBattles(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Battles during turn:") ;

        public static bool MatchBattle(this Cursor<MultiLineBlock> cursor)
            => battle.IsMatch(cursor.Value.Text);

        public static bool MatchErrors(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Errors during turn:");

        public static bool MatchEvents(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Events during turn:");

        public static bool MatchSkills(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Skill reports:");

        public static bool MatchItems(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Item reports:");

        public static bool MatchObjects(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Object reports:");

        public static bool MatchDefaultAttitude(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Declared Attitudes");

        public static bool MatchAttitue(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Hostile")
            || cursor.Value.Text.StartsWithIC("Unfriendly")
            || cursor.Value.Text.StartsWithIC("Neutral")
            || cursor.Value.Text.StartsWithIC("Friendly")
            || cursor.Value.Text.StartsWithIC("Ally");

        public static bool MatchUnclaimed(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Unclaimed silver:");

        public static async Task<bool> MatchRegionStart(this Cursor<MultiLineBlock> cursor) {
            bool isRegion = false;
            if (await cursor.NextAsync()) {
                isRegion = cursor.Value.Text.StartsWith("-----");
            }

            cursor.Back();

            return isRegion;
        }

        public static bool MatchOrdersTemplate(this Cursor<MultiLineBlock> cursor)
            => cursor.Value.Text.StartsWithIC("Orders Template");

        public static bool MatchStructure(this Cursor<MultiLineBlock> cursor) => cursor.Value.Text.StartsWith("+");
        public static bool MatchOwnUnit(this Cursor<MultiLineBlock> cursor) => cursor.Value.Text.StartsWith("*");
        public static bool MatchUnit(this Cursor<MultiLineBlock> cursor) => cursor.Value.Text.StartsWith("-");
        public static bool MatchRegionExits(this Cursor<MultiLineBlock> cursor) => cursor.Value.Text.StartsWithIC("Exits:");
        public static bool MatchRegionInfo(this Cursor<MultiLineBlock> cursor) => cursor.Value.Text.StartsWithIC("-----");
        public static bool MatchRegionGate(this Cursor<MultiLineBlock> cursor) => cursor.Value.Text.StartsWithIC("There is a Gate here");
    }
}
