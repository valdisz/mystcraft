namespace atlantis
{
    public static class AllParsers {
        public static readonly FactionNameParser FactionName = new FactionNameParser();
        public static readonly FactionArgumentParser FactionArgument = new FactionArgumentParser();
        public static readonly ReportFactionParser ReportFaction = new ReportFactionParser(FactionName, FactionArgument);
        public static readonly ReportDateParser ReportDate = new ReportDateParser();
        public static readonly FactionStatusItemParser FactionStatusItem = new FactionStatusItemParser();
        public static readonly SkillParser Skill = new SkillParser();
        public static readonly ItemParser Item = new ItemParser();
        public static readonly CoordsParser Coords = new CoordsParser();
        public static readonly RegionHeaderParser RegionHeader = new RegionHeaderParser(Coords);
        public static readonly UnitParser Unit = new UnitParser(Skill);
    }
}
