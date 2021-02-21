namespace atlantis.Model {
    public class JRegion {
        public string Terrain { get; set; }
        public JCoords Coords { get; set; }
        public string Province { get; set; }
        public JSettlement Settlement { get; set; }
        public JPopulation Population { get; set; }
        public int? Tax { get; set; }
        public double Wages { get; set; }
        public int? TotalWages { get; set; }
        public int? Entertainment { get; set; }
        public JTradableItem[] Wanted { get; set; } = new JTradableItem[0];
        public JTradableItem[] ForSale { get; set; } = new JTradableItem[0];
        public JItem[] Products { get; set; } = new JTradableItem[0];
        public JExit[] Exits { get; set; } = new JExit[0];
        public JUnit[] Units { get; set; }= new JUnit[0];
        public JStructure[] Structures {get; set; }= new JStructure[0];
    }

    public class JFaction {
        public string Name { get; set; }
        public int Number { get; set; }
    }

    public class JCapacity {
        public int Flying { get; set; }
        public int Riding { get; set; }
        public int Walking { get; set; }
        public int Swimming { get; set; }
    }

    public class JSkill {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class JKnownSkill : JSkill {
        public int Level { get; set; }
        public int Days { get; set; }
    }

    public class JUnit {
        public bool Own { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public JFaction Faction { get; set; }
        public string Description { get; set; }
        public bool OnGuard { get; set; }
        public string[] Flags { get; set; }
        public JItem[] Items { get; set; }
        public int? Weight { get; set; }
        public JCapacity Capacity { get; set; }
        public JKnownSkill[] Skills { get; set; }
        public JSkill[] CanStudy { get; set; }
        public JItem ReadyItem { get; set; }
        public JSkill CombatSpell { get; set; }
    }

    public class JFleetContent {
        public int Count { get; set; }
        public string Type { get; set; }
    }

    public class JTransportationLoad {
        public int Used { get; set; }
        public int Max { get; set; }
    }

    public class JSailors {
        public int Current { get; set; }
        public int Required { get; set; }
    }

    public class JStructureInfo {
        public int Number { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public JFleetContent[] Contents { get; set; }
        public string[] Flags { get; set; }
        public Direction[] SailDirections { get; set; }
        public int? Speed { get; set; }
        public int? Needs { get; set; }
        public JTransportationLoad Load { get; set; }
        public JSailors Sailors { get; set; }
    }

    public class JStructure {
        public JStructureInfo Structure { get; set; }
        public JUnit[] Units {get; set; } = new JUnit[0];
    }
}
