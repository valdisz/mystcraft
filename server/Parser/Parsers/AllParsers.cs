namespace advisor
{
    public static class AllParsers {
        public static readonly FactionNameParser FactionName = new FactionNameParser();
        public static readonly ReportFactionParser ReportFaction = new ReportFactionParser(FactionName, new FactionArgumentParser());
        public static readonly ReportDateParser ReportDate = new ReportDateParser();
        public static readonly FactionStatusItemParser FactionStatusItem = new FactionStatusItemParser();
        public static readonly SkillParser Skill = new SkillParser();
        public static readonly ItemParser Item = new ItemParser();
        public static readonly CoordsParser Coords = new CoordsParser();
        public static readonly LocationParser Location = new LocationParser(Coords);
        public static readonly SettlementParser Settlement = new SettlementParser();
        public static readonly RegionHeaderParser RegionHeader = new RegionHeaderParser(Location, Settlement);
        public static readonly UnitParser Unit = new UnitParser(Skill, Item);
        public static readonly StructureParser Structure = new StructureParser();
        public static readonly RegionPropsParser RegionProps = new RegionPropsParser(Item);
        public static readonly RegionExistsParser RegionExits = new RegionExistsParser(Location, Settlement);
        public static readonly RegionGateParser RegionGate = new RegionGateParser();
        public static readonly EventParser Event = new EventParser(new UnitNameParser(), Location, Item);
    }
}
