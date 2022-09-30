namespace advisor.Model {
    using System.Collections.Generic;
    using HotChocolate;

    [GraphQLName("BattleFaction")]
    public class JBattleFaction {
        public string Name { get; set; }
        public int Number { get; set; }
    }

    [GraphQLName("BattleSkill")]
    public class JBattleSkill {
        public string Name { get; set; }
        public int Level { get; set; }
    }

    public class JBattleItem : AnItem {
        public string Code { get; set; }
        public int Amount { get; set; }
    }

    [GraphQLName("BattleUnit")]
    public class JBattleUnit {
        public string Name { get; set; }
        public int Number { get; set; }
        public JBattleFaction Faction { get; set; }
        public string Description { get; set; }
        public List<string> Flags { get; set; } = new ();
        public List<JBattleItem> Items { get; set; } = new ();
        public List<JBattleSkill> Skills { get; set; } = new ();
    }

    [GraphQLName("Battle")]
    public class JBattle {
        public JParticipant Attacker { get; set; }
        public JParticipant Defender { get; set; }
        public JLocation Location { get; set; }
        public List<JBattleUnit> Attackers { get; set; }
        public List<JBattleUnit> Defenders { get; set; }
        public List<JBattleRound> Rounds { get; set; }
        public string Statistics { get; set; }
        public List<JCasualties> Casualties { get; set; }
        public List<JBattleItem> Spoils { get; set; }
        public List<JBattleItem> Rose { get; set; }
    }
}
