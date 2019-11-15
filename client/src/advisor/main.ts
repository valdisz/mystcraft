import { Faction, Item, ItemWithPrice, Unit, Capacity, Skill, Attribute, ItemBase, SkillBase, Region, Coords, Population, Settlement, Wages, Structure } from "./types";

/**
 * The Corporation object acts as a scheduler, running directives and departaments for all branches each tick. It is also
 * in charge of starting new "processes" (directives) to respond to various situations.
 */
export class Corporation {

}

/**
 * Branches are the highest-level object other than the global Corporation. A branch groups together all hexes, structures,
 * units, utilities, etc. which are run from a single location.
 *
 * Branch creates semi-isolated economy with extraction, production, warehousing and supply lines independent from other
 * branches. This is suitable when economic activity is done in remote areas and needs local administration center for
 * faster reaction time.
 *
 * Branches can share excess resources with each other.
 *
 * Army and fleet can be managed by branch. Such branch will put high priority requests for other branches to supply its
 * needs.
 */
export class Branch {

}

/**
 * Directives are contextual wrappers for flags and serve as attachment points for Departaments, acting as a sort of
 * "process table" for the bot, with individual processes (Departaments) run by the scheulder (Corporation)
 */
export class Directive {

}

/**
 * An Departament is roughly analogous to a process in an OS: it is a generalization of a set of related things that need
 * to be done in a branch, like mining from a site, bootstrapping a new branch, guarding against invaders, or building
 * construction sites. Departaments handle spawning or obtaining suitable units to do these things and contain the actual
 * implementation of doing them.
 */
export class Departament {

}

/**
 * An abstract class for encapsulating unit actions. This generalizes the concept of "do action X to thing Y until
 * condition Z is met" and saves a lot of convoluted and duplicated code in unit logic. A Task object contains
 * the necessary logic for traveling to a target, performing a task, and realizing when a task is no longer sensible
 * to continue.
 */
export class Task {

}

export interface Race {
    code: string;
}

export const RACES: { [code: string]: Race } = {
    'LEAD': { code: 'LEAD' },
    'WELF': { code: 'WELF' },
    'HELF': { code: 'HELF' },
    'IDWA': { code: 'IDWA' },
    'HDWA': { code: 'HDWA' },
    'UDWA': { code: 'UDWA' },
    'ORC':  { code: 'ORC' },
    'GNOM': { code: 'GNOM' },
    'CTAU': { code: 'CTAU' },
    'MAN':  { code: 'MAN' },
    'LIZA': { code: 'LIZA' },
    'GBLN': { code: 'GBLN' },
    'GNOL': { code: 'GNOL' },
    'DRLF': { code: 'DRLF' }
};

// export type ItemInfo =

export enum ItemCategory {
    SILVER,
    MAN,
    ARMOR
}

export enum ItemClass {
    SILVER,
    MAN,
    ADVANCED
}

export type TraitType = 'can-study' | 'can-study-all' | 'movement' | 'consumption' | 'hitpoints' | 'producable' | 'protect';

export interface Trait {
    readonly type: TraitType;
}


export interface CanStudyProps {
    skillCode: string;
    level: number;
}

export class CanStudy implements Trait {
    readonly type: TraitType = 'can-study';

    constructor(props: CanStudyProps) {
        this.skillCode = props.skillCode;
        this.level = props.level;
    }

    readonly skillCode: string;
    readonly level: number;
}


export interface CanStudyAllProps {
    level: number;
}

export class CanStudyAll implements Trait {
    readonly type: TraitType = 'can-study-all';
    constructor(props: CanStudyAllProps) {
        this.level = props.level;
    }

    readonly level: number;
}


export interface MovementProps {
    walk?: number;
    ride?: number;
    fly?: number;
    swim?: number;
    movePoints: number;
}

export class Movement implements Trait {
    readonly type: TraitType = 'movement';
    constructor(props: MovementProps) {
        this.walk = props.walk || 0;
        this.ride = props.ride || 0;
        this.fly = props.fly || 0;
        this.swim = props.swim || 0;
        this.movePoints = props.movePoints;
    }

    readonly walk: number;
    readonly ride: number;
    readonly fly: number;
    readonly swim: number;
    readonly movePoints: number
}


export interface ConsumptionProps {
    itemCode: string;
    amount: number;
}

export class Consumption implements Trait {
    readonly type: TraitType = 'consumption';
    constructor(props: ConsumptionProps) {
        this.itemCode = props.itemCode;
        this.amount = props.amount;
    }

    readonly itemCode: string;
    readonly amount: number;
}


export interface HitpointsProps {
    hitpoints: number;
}

export class Hitpoints implements Trait {
    readonly type: TraitType = 'hitpoints';
    constructor(props: HitpointsProps) {
        this.hitpoints = props.hitpoints;
    }

    readonly hitpoints: number;
}


export class Producable implements Trait {
    readonly type: TraitType = 'producable';

    constructor(public item: ItemBase) {}
}

export enum DamageClass {
    SLASHING,
    CLEAVING,
    CRUSHING,
    PIERCING,
    ARMOR_PIERCING,
    ENERGY,
    SPIRIT,
    WEATHER
}

export enum AttackClass {
    MELEE,
    RIDING,
    RANGED,
    ENERGY,
    SPIRIT,
    WEATHER
}

export interface ProtectProps {
    damage: DamageClass;
    protection: number;
}

export class Protect implements Trait {
    readonly type: TraitType = 'protect';
    constructor(props: ProtectProps) {
        this.damage = props.damage;
        this.protection = props.protection;
    }

    readonly damage: DamageClass;
    readonly protection: number;
}

export type Traits = CanStudy | CanStudyAll | Movement | Consumption | Hitpoints | Protect;

export interface ItemInfo {
    code: string;
    singular: string;
    plural: string;
    weight: number;
    description: string;
    itemCategory: ItemCategory;
    itemClass: ItemClass;
    traits: Traits[];
}


const items: { [code: string]: ItemInfo } = {
    'SILV': {
        code: 'SILV',
        singular: 'silver',
        plural: 'silver',
        description: 'Silver',
        itemCategory: ItemCategory.SILVER,
        itemClass: ItemClass.SILVER,
        weight: 0,
        traits: []
    },
    'HELF': {
        code: 'HELF',
        singular: 'high elf',
        plural: 'high elves',
        description: 'Weight 10, walking capacity 5, moves 2 hexes per month. This race may study horse training [HORS], combat [COMB], longbow [LBOW], shipbuilding [SHIP] and sailing [SAIL] to level 5 and all other skills to level 2.',
        itemCategory: ItemCategory.MAN,
        itemClass: ItemClass.MAN,
        weight: 10,
        traits: [
            new Hitpoints({ hitpoints: 1 }),
            new Consumption({ itemCode: 'SILV', amount: 10 }),
            new Movement({ walk: 5, movePoints: 2 }),
            new CanStudyAll({ level: 2 }),
            new CanStudy({ skillCode: 'HORS', level: 5 }),
            new CanStudy({ skillCode: 'COMB', level: 5 }),
            new CanStudy({ skillCode: 'LBOW', level: 5 }),
            new CanStudy({ skillCode: 'SHIP', level: 5 }),
            new CanStudy({ skillCode: 'SAIL', level: 5 }),
        ]
    },
    'MARM': {
        code: 'MARM',
        singular: 'mithril armor',
        plural: 'mithril armors',
        description: `mithril armor [MARM], weight 1. This is a type of armor. This armor
        will protect its wearer 90% of the time versus slashing attacks, 90%
        of the time versus piercing attacks, 90% of the time versus crushing
        attacks, 90% of the time versus cleaving attacks, 67% of the time
        versus armor-piercing attacks, 67% of the time versus energy
        attacks, 67% of the time versus spirit attacks, and 67% of the time
        versus weather attacks.`,
        itemCategory: ItemCategory.ARMOR,
        itemClass: ItemClass.ADVANCED,
        weight: 1,
        traits: [
            new Protect({ damage: DamageClass.SLASHING, protection: 0.9 }),
            new Protect({ damage: DamageClass.PIERCING, protection: 0.9 }),
            new Protect({ damage: DamageClass.SLASHING, protection: 0.9 }),
            new Protect({ damage: DamageClass.SLASHING, protection: 0.9 }),
            new Protect({ damage: DamageClass.SLASHING, protection: 0.9 }),
            new Protect({ damage: DamageClass.SLASHING, protection: 0.9 }),
        ]
    }
};


export class AItem implements Item, ItemWithPrice {
    constructor(item: Item | ItemWithPrice, itemInfo: ItemInfo) {
        this.itemInfo = itemInfo;
        this.code = item.code;
        this.amount = item.amount;
        this.name = item.name;
        this.needs = item.needs || 0;
        this.price = (item as ItemWithPrice).price || 0;
    }

    readonly itemInfo: ItemInfo;
    readonly code: string;
    readonly amount: number;
    readonly name: string;
    readonly needs: number;
    readonly price: number;

    get isMan(): boolean {
        return this.itemInfo.itemClass === ItemClass.MAN;
    }

    get isSilver(): boolean {
        return this.itemInfo.itemClass === ItemClass.SILVER;
    }

    get isUnlimited() {
        return this.amount < 0;
    }
}

export abstract class Order {

}

export class GiveOrder extends Order {

}

export class TakeOrder extends Order {

}

export class MoveOrder extends Order {

}

export class WorkOrder extends Order {

}

export class UnknownOrder extends Order {

}

export type Orders = GiveOrder | TakeOrder | MoveOrder | WorkOrder | UnknownOrder;

export class ItemFactory {
    createItem(item: Item): AItem {
        return new AItem(item, null);
    }
}

export class AUnit implements Unit {
    constructor(unit: Unit, itemFactory: ItemFactory) {
        this.name = unit.name;
        this.number = unit.number;
        this.faction = unit.faction ? new AFaction(unit.faction) : null;
        this.description = unit.description;
        this.items = (unit.items || []).map(i => itemFactory.createItem(i));
        this.own = unit.own;
        this.flags = unit.flags;
        this.wieght = unit.weight || 0;
        this.capacity = unit.capacity || {
            flying: 0,
            riding: 0,
            swimming: 0,
            walking: 0
        };
        this.skills = unit.skills || [];
        this.canStudy = unit.canStudy || [];
        this.attributes = unit.attributes || [];
    }

    readonly name: string;
    readonly number: number;
    readonly faction: AFaction | null;
    readonly description: string;
    readonly items: AItem[];
    readonly own: boolean;
    readonly flags: string[];
    readonly wieght: number;
    readonly capacity: Capacity;
    readonly skills: Skill[];
    readonly canStudy: SkillBase[];
    readonly attributes: Attribute[];

    readonly orders: Orders[] = [];

    addOrder(order: Orders) {
        this.orders.push(order);
    }

    removeOrder(i: number) {
        this.orders.splice(i, 1);
    }

    replaceOrder(i: number, order: Orders) {
        this.orders[i] = order;
    }
}

export class AProvince {

}

export class ARegion {
    constructor(region: Region, itemFactory: ItemFactory) {
        this.terrain = region.regionHeader.location.terrain;
        this.coords = region.regionHeader.location.coords;
        this.prvonice = region.regionHeader.location.province;
        this.population = region.regionHeader.population || null;
        this.tax = region.regionHeader.tax || 0;
        this.settlement = region.regionHeader.settlement || null;
        this.wages = region.wages || {
            maxWages: 0,
            salary: 0
        };
        this.wanted = region.wanted.map(i => itemFactory.createItem(i));
        this.forSale = region.forSale.map(i => itemFactory.createItem(i));
        this.products = region.products.map(i => itemFactory.createItem(i));
        this.entertainment = region.entertainment || 0;
        this.units = region.units.map(u => new AUnit(u, itemFactory));
        this.structures = region.structures || [];
        this.attributes = region.attributes || [];
    }

    readonly terrain: string;
    readonly coords: Coords;
    readonly prvonice: string;
    readonly population: Population | null;
    readonly tax: number;
    readonly settlement: Settlement | null;
    readonly wages: Wages;
    readonly wanted: AItem[];
    readonly forSale: AItem[];
    readonly products: AItem[];
    readonly entertainment: number;
    readonly units: AUnit[];
    readonly structures: Structure[];
    readonly attributes: Attribute[];
}

export class AFaction implements Faction {
    constructor(faction: Faction) {
        this.number = faction.number;
        this.name = faction.name;
    }

    readonly number: number;
    name: string;
}

export class AGame {

}
