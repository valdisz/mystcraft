import { RegionFragment } from '../../schema'
import { Item, ItemInfo } from './item'
import { List } from './list'
import { UniqueItem } from './unique-item'

export class Capacity {
    walk: number
    ride: number
    fly: number
    swim: number
}
export type MoveType = keyof Capacity

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

interface Coords {
    x: number
    y: number
    z: number
}

export class TerrainInfo implements UniqueItem {
    constructor(public readonly code: string) {

    }
}

export class Structure {
    constructor(public readonly region: Region) {

    }

    readonly units: Unit[] = []
}

export class Region {
    province: Province
    
    readonly terrain: TerrainInfo
    readonly coords: Coords
    // readonly population: Population | null;
    readonly tax: number
    // readonly settlement: Settlement | null;
    // readonly wages: Wages;
    readonly wanted: List<Item>
    readonly forSale: List<Item>
    readonly products: List<Item>
    readonly entertainment: number

    readonly units: Unit[] = []
    readonly structures: Structure[] = []
}

export interface TypedMap<T> {
    [name: string]: T
}

export class Province {
    constructor(public readonly name: string) {

    }

    regions: Region[]

    add(region: Region) {
        this.regions.push(region)
        region.province = this
    }
}

export class Provinces {
    private readonly provinces: TypedMap<Province> = { }

    get(name: string) {
        return this.provinces[name]
    }

    create(name: string) {
        const province = new Province(name)
        this.provinces[name] = province
        return province
    }
}

export class World {
    readonly levels: string[] = [ ]

    readonly regions: Region[] = []
    readonly provinces = new Provinces()

    readonly itemInfo: List<ItemInfo> = new List<ItemInfo>()
    readonly terrainInfo: List<TerrainInfo> = new List<TerrainInfo>()

    addRegions(regions: RegionFragment[]) {
        for (const reg of regions) {
            this.addRegion(reg)
        }
    }

    addRegion(region: RegionFragment) {
        const province = this.provinces.get(region.province) ?? this.provinces.create(region.province)

        const reg = new Region()
        province.add(reg)

        
    }
}
