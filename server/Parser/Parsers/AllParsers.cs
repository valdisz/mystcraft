namespace atlantis
{
    public static class AllParsers {
        public static readonly FactionNameParser FactionName = new FactionNameParser();
        public static readonly ReportFactionParser ReportFaction = new ReportFactionParser(FactionName, new FactionArgumentParser());
        public static readonly ReportDateParser ReportDate = new ReportDateParser();
        public static readonly FactionStatusItemParser FactionStatusItem = new FactionStatusItemParser();
        public static readonly SkillParser Skill = new SkillParser();
        public static readonly ItemParser Item = new ItemParser();
        public static readonly RegionHeaderParser RegionHeader = new RegionHeaderParser(new CoordsParser());
        public static readonly UnitParser Unit = new UnitParser(Skill);
        public static readonly StructureParser Structure = new StructureParser();
    }
}
