import { UnitFragment } from '../../schema';
import { Item } from './item';
import { ItemInfo } from "./item-info";
import { List } from './list';
import { Ruleset } from './ruleset';
import { Capacity } from './move-capacity';
import { Faction } from "./faction";
import { Flag } from "./flag";
import { Skill } from "./skill";
import { SkillInfo } from "./skill-info";
import { Order } from "./order";
import { Event } from "./event";
import { Inventory } from "./inventory";
import { Region, Structure } from './types';

export class Unit {
    constructor(
        public readonly id: string,
        public readonly num: number,
        public name: string,
        private readonly ruleset: Ruleset) {
    }

    readonly inventory = new Inventory(this);

    faction?: Faction;
    // troops: Troops;
    region: Region;
    structure?: Structure;

    description?: string;
    onGuard = false;
    readonly flags: Flag[] = [];
    readonly skills = new List<Skill>();
    readonly canStudy = new List<SkillInfo>();
    weight = 0;
    readonly capacity = new Capacity();
    readyItem: ItemInfo;
    combatSpell: SkillInfo;
    readonly events: Event[] = [];
    readonly orders: Order[] = [];

    get money() {
        return this.inventory.items.get(this.ruleset.money)?.amount ?? 0
    }

    static from(src: UnitFragment, ruleset: Ruleset) {
        const unit = new Unit(src.id, src.number, src.name, ruleset)

        unit.description = src.description
        if (unit.description && !unit.description.endsWith('.')) unit.description += '.'

        unit.onGuard = src.onGuard

        for (const flag of src.flags ?? []) {
            unit.flags.push(flag)
        }

        for (const { code, amount } of src.items) {
            const item = ruleset.getItem(code).create(amount ?? 1)
            unit.inventory.items.set(item)
        }

        for (const { code, days, level } of src.skills ?? []) {
            const skill = ruleset.getSkill(code).create(days, level)
            unit.skills.set(skill)
        }

        for (const { code } of src.canStudy) {
            const skillInfo = ruleset.getSkill(code)
            unit.canStudy.set(skillInfo)
        }

        unit.readyItem = src.readyItem
             ? ruleset.getItem(src.readyItem.code)
             : null

        unit.combatSpell = src.combatSpell
            ? ruleset.getSkill(src.combatSpell.code)
            : null

        unit.weight = src.weight
            ? src.weight
            : unit.inventory.items.all.map(x => x.weight).reduce((w, v) => w + v)

        if (src.capacity) {
            const { walking, swimming, riding, flying } = src.capacity
            unit.capacity.walk = walking
            unit.capacity.swim = swimming
            unit.capacity.ride = riding
            unit.capacity.fly = flying
        }
        else {
            for (const item of unit.inventory.items.all) {
                const { canMove } = item.info.traits
                if (canMove) {
                    const { requires } = canMove
                    if (requires && !unit.inventory.items.contains(requires.code)) {
                        continue
                    }

                    unit.capacity.add(canMove.capacity)
                }
            }
        }

        return unit
    }
}
