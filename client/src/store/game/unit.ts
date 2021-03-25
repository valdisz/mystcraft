import { UnitFragment } from '../../schema';
import { Item } from './item';
import { ItemInfo } from "./item-info";
import { List } from './list';
import { Region } from './region';
import { Ruleset } from './ruleset';
import { Structure } from './structure';
import { Capacity } from './move-capacity';
import { Faction } from "./faction";
import { Flag } from "./flag";
import { Skill } from "./skill";
import { SkillInfo } from "./skill-info";
import { Order } from "./order";
import { Event } from "./event";
import { Inventory } from "./inventory";


export class Unit {
    constructor(
        public readonly id: string,
        public readonly region: Region,
        public structure: Structure,
        public readonly own: boolean,
        public readonly num: number,
        public name: string) {
    }

    readonly inventory = new Inventory(this);

    faction?: Faction;
    description?: string;
    onGuard = false;
    flags: Flag[] = [];
    readonly men = new List<Item>();
    silver: number = 0;
    readonly skills = new List<Skill>();
    readonly canStudy = new List<SkillInfo>();
    weight = 0;
    readonly capacity = new Capacity();
    readyItem: ItemInfo;
    combatSpell: SkillInfo;
    readonly events: Event[] = [];
    readonly orders: Order[] = [];
    memory: any;

    static from(unit: UnitFragment, ruleset: Ruleset) {
    }
}
