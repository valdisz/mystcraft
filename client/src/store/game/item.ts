const SILVER = 'SILV'

export interface UniqueItem {
    readonly code: string
}

export class Item implements UniqueItem {
    constructor(public readonly info: ItemInfo) {

    }

    get name() {
        return this.amount > 1
            ? this.info.name[1]
            : this.info.name[0]
    }

    get code() {
        return this.info.code
    }

    amount: number
    price: number

    get weight() {
        return this.amount * this.info.weight
    }

    trait(trait: Traits) {
        return this.info.trait(trait)
    }

    hasTrait(trait: Traits) {
        return this.info.hasTrait(trait)
    }
}

export class Capacity {
    walk: number
    ride: number
    fly: number
    swim: number
}
export type MoveType = keyof Capacity

export type Traits = 'silver' | 'man'

export abstract class Trait {
    abstract readonly type: Traits
}

// this is silver
export class SilverTrait extends Trait {
    readonly type: Traits = 'silver'
}

// this is man
export class ManTrait extends Trait {
    readonly type: Traits = 'man'
}

export type AllTraits = SilverTrait | ManTrait

export type TypeOfTrait = keyof Trait

export type TraitsMap = {
    [ trait in Traits ]: AllTraits
}

export class ItemInfo implements UniqueItem {
    constructor(public readonly code: string, traits: AllTraits[]) {

    }

    private readonly traits: TraitsMap

    weight: number
    name: [string, string]
    description: string

    trait(trait: Traits): AllTraits | undefined {
        return this.traits[trait]
    }

    hasTrait(trait: Traits) {
        return !!this.traits[trait]
    }

    create(): Item {
        return new Item(this)
    }
}

export class Faction {
    num: number
    name: string
    player: boolean
}

export class Flag {

}

export class Skill implements UniqueItem {
    constructor(public readonly info: SkillInfo) {

    }

    get name() {
        return this.info.name
    }

    get code() {
        return this.info.code
    }

    level: number
    days: number
}

export class SkillInfo implements UniqueItem {
    constructor(public readonly code: string) {

    }

    name: string
    description: string

    create(): Skill {
        return new Skill(this)
    }
}

export class Order {

}

export class Event {


}

export class List<T extends UniqueItem> {
    constructor(items?: T[]) {
        if (!items) return

        for (const item of items) {
            this.set(item)
        }
    }

    private readonly items: {
        [code: string]: T
    } = { }

    get length() {
        return Object.keys(this.items).length
    }

    get all() {
        return Object.values(this.items)
    }

    get(code: string): T {
        return this.items[code]
    }

    set(item: T): void {
        this.items[item.code] = item
    }

    contains(item: T | string) {
        return !!this.items[typeof item === 'string' ? item : item.code]
    }

    remove(item: T | string) {
        const code = typeof item === 'string'
            ? item
            : item.code

        if (this.items[code]) {
            delete this.items[code]
        }
    }
}

export interface Transfer {
    target: Unit
    item: ItemInfo
    amount: number
}

export interface Income {
    tax: number
    sell: number
    entertain: number
    work: number
}

export enum TransferOutcome {
    Ok,
    NotEnaugh,
    NoItem
}

export class Inventory {
    constructor(public readonly owner: Unit, items?: Item[]) {

    }

    private readonly items = new List<Item>()

    readonly credit: Transfer[] = []
    readonly debit: Transfer[] = []

    readonly balance = new List<Item>()

    readonly income: Income = {
        tax: 0,
        sell: 0,
        entertain: 0,
        work: 0
    }

    transfer(target: Unit, itemOrCode: ItemInfo | string, amount?: number) {
        const code = typeof itemOrCode === 'string' ? itemOrCode : itemOrCode.code
        const item = this.balance.get(code)
        const src = this.items.get(code)

        if (!item) return TransferOutcome.NoItem
        if (amount && item.amount < amount) return TransferOutcome.NotEnaugh

        if (!amount) amount = item.amount
        if (!src || src.amount < amount) {
            // todo: rewrite incoming transfers
        }

        this.debit.push({ amount, target, item: item.info })
        target.inventory.credit.push({ amount, target: this.owner, item: item.info })

        // todo: update balance
    }

    receive(source: Unit, item: ItemInfo, amount: number) {

    }
}

export class Unit {
    constructor(
        public readonly region: Region,
        public structure: Structure,
        public readonly own: boolean,
        public readonly num: number,
        public name: string ) {

    }

    readonly inventory = new Inventory(this)

    faction?: Faction
    description?: string
    onGuard = false
    flags: Flag[] = []
    readonly items = new List<Item>()
    readonly men = new List<Item>()
    silver: number = 0
    readonly skills = new List<Skill>()
    readonly canStudy = new List<SkillInfo>()
    weight = 0
    readonly capacity = new Capacity()
    readyItem: ItemInfo
    combatSpell: SkillInfo
    readonly events: Event[] = []
    readonly orders: Order[] = []
    memory: any
}

export class Company {
    constructor(public readonly faction: Faction | null, public readonly units: Unit[]) {

    }
}

export class Region {
    readonly units: Unit[] = []
    readonly structures: Structure[] = []
}

export class Structure {
    constructor(public readonly region: Region) {

    }

    readonly units: Unit[] = []
}

export class World {
    readonly levels: string[] = [ ]
    readonly regions: Region[] = []
    readonly factions: Faction[] = []
    readonly playerFaction: Faction

    getRegion(x: number, y: number, z: number) {

    }
}
