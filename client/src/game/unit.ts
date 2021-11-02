import { UnitFragment } from '../schema';
import { ItemInfo } from "./item-info";
import { ItemMap } from './item-map';
import { Ruleset } from './ruleset';
import { Capacity, MoveType } from './move-capacity';
import { Faction, Factions } from "./faction";
import { Flag } from "./flag";
import { Skill } from "./skill";
import { SkillInfo } from "./skill-info";
import { Event } from "./event";
import { Inventory } from "./inventory";
import { Region, Structure } from './types';
import { OrderParser, ORDER_PARSER, UnitOrder, Order } from './orders/parser';

export class Unit {
    constructor(
        public readonly id: string,
        public readonly num: number,
        public name: string,
        private readonly ruleset: Ruleset) {
    }

    readonly inventory = new Inventory();

    seq: number
    faction: Faction;
    // troops: Troops;
    region: Region;
    structure?: Structure;
    get isPlayer() {
        return this.faction.isPlayer
    }

    description?: string;
    onGuard = false;
    readonly flags: Flag[] = [];
    readonly skills = new ItemMap<Skill>();
    readonly canStudy = new ItemMap<SkillInfo>();
    weight = 0;
    readonly capacity = new Capacity();
    readyItem: ItemInfo;
    combatSpell: SkillInfo;
    readonly events: Event[] = [];

    ordersSrc: string
    orders: UnitOrder[] = []

    setOrders(orders: string, parser: OrderParser = null) {
        this.ordersSrc = orders
        this.parseOrders(parser)
    }

    parseOrders(parser: OrderParser = null) {
        if (this.ordersSrc) {
            this.orders = (parser ?? ORDER_PARSER).parse(this.ordersSrc)
        }
        else {
            this.orders = []
        }
    }

    get money() {
        return this.inventory.items.get(this.ruleset.money)?.amount ?? 0
    }

    get canSwim() {
        return this.weight > 0 && this.capacity.swim >= this.weight
    }

    get movement(): MoveType | null {
        if (this.weight == 0) return null

        if (this.capacity.fly >= this.weight) return 'fly'
        if (this.capacity.ride >= this.weight) return 'ride'
        if (this.capacity.walk >= this.weight) return 'walk'

        return null
    }

    get isOverweight() {
        return this.weight > 0 && this.movement === null
    }

    static from(src: UnitFragment, factions: Factions, ruleset: Ruleset) {
        const unit = new Unit(src.id, src.number, src.name, ruleset)
        unit.seq = src.sequence

        if (src.factionNumber) {
            const faction = factions.get(src.factionNumber)
            unit.faction = faction
        }
        else {
            unit.faction = factions.unknown
        }

        unit.faction.troops.add(unit)

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

        for (const code of src.canStudy) {
            const skillInfo = ruleset.getSkill(code)
            unit.canStudy.set(skillInfo)
        }

        unit.readyItem = src.readyItem
             ? ruleset.getItem(src.readyItem)
             : null

        unit.combatSpell = src.combatSpell
            ? ruleset.getSkill(src.combatSpell)
            : null

        unit.weight = src.weight
            ? src.weight
            : unit.inventory.items.all.map(x => x.weight).reduce((w, v) => w + v)

        unit.setOrders(src.orders)

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
                    if (requires && !unit.inventory.items.has(requires.code)) {
                        continue
                    }

                    addCapacity(unit.capacity, 'fly', canMove.capacity, item.amount, item.weight)
                    addCapacity(unit.capacity, 'ride', canMove.capacity, item.amount, item.weight)
                    addCapacity(unit.capacity, 'walk', canMove.capacity, item.amount, item.weight)
                    addCapacity(unit.capacity, 'swim', canMove.capacity, item.amount, item.weight)
                }
            }
        }

        return unit
    }
}

function addCapacity(target: Capacity, moveType: MoveType, itemCapacity: Capacity, itemCount: number, itemOwnWeigh: number) {
    const cap = itemCapacity[moveType]
    if (cap) {
        target[moveType] += (cap * itemCount) + itemOwnWeigh
    }
}
