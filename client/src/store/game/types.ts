import { RegionFragment, UnitFragment } from '../../schema'
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
        public readonly id: string,
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

    static from(unit: UnitFragment, ruleset: Ruleset) {

    }
}

export class Company {
    constructor(public readonly faction: Faction | null, public readonly units: Unit[]) {

    }
}

interface Coords {
    x: number
    y: number
    z: number
    label?: string
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

export type SettlementSize = 'village' | 'town' | 'city';

export interface Settlement {
    name: string
    size: SettlementSize
}

export interface Population {
    amount: number
    race: ItemInfo
}

export class Ruleset {
    readonly items: List<ItemInfo> = new List()
    readonly terrain: List<TerrainInfo> = new List()
    
    getItem(nameOrCode: string) {
        let item = this.items.get(nameOrCode)

        if (!item) {
            for (const i of this.items.all) {
                if (item) break

                for (const name of i.name) {
                    if (nameOrCode === name) {
                        item = i
                        break
                    }
                }
            }
        }

        // we must ensure that even missing items from the ruleset does not crash the client
        if (!item) {
            item = new ItemInfo(nameOrCode, nameOrCode, nameOrCode, [])
            this.items.set(item)
        }

        return item
    }

    getTerrain(name: string) {
        let t = this.terrain.get(name)

        // we must ensure that even missing terrain from the ruleset does not crash the client
        if (!t) {
            t = new TerrainInfo(name)
            this.terrain.set(t)
        }

        return t
    }
}

export interface Wages {
    amount: number
    total: number
}

export class Region {
    constructor(public readonly id: string, public readonly coords: Coords) {
        this.key = `${coords.x} ${coords.y} ${coords.z}`
    }

    readonly key: string

    province: Province
    terrain: TerrainInfo
    population: Population | null
    settlement: Settlement | null
    tax: number
    wages: Wages
    entertainment: number
    readonly wanted: List<Item> = new List()
    readonly forSale: List<Item> = new List()
    readonly products: List<Item> = new List()

    readonly units: Unit[] = []
    readonly structures: Structure[] = []

    static from(src: RegionFragment, ruleset: Ruleset) {
        const reg = new Region(src.id, {
            x: src.x,
            y: src.y,
            z: src.z,
            label: src.label
        })

        reg.population = {
            amount: src.population,
            race: ruleset.getItem(src.race)
        }

        reg.terrain = ruleset.getTerrain(src.terrain)

        reg.tax = src.tax
        reg.wages = {
            amount: src.wages,
            total: src.totalWages
        }
        reg.entertainment = src.entertainment

        for (const prod of src.products) {
            const item = ruleset.getItem(prod.code).create()
            item.amount = prod.amount
            
            reg.products.set(item)
        }

        for (const sale of src.forSale) {
            const item = ruleset.getItem(sale.code).create()
            item.amount = sale.amount
            item.price = sale.price
            
            reg.forSale.set(item)
        }

        for (const wanted of src.wanted) {
            const item = ruleset.getItem(wanted.code).create()
            item.amount = wanted.amount
            item.price = wanted.price
            
            reg.wanted.set(item)
        }

        return reg
    }
}

export interface TypedMap<T> {
    [name: string]: T
}

export class Province {
    constructor(public readonly name: string) {

    }

    readonly regions: Region[] = []

    add(region: Region) {
        this.regions.push(region)
        region.province = this
    }
}

export class Provinces {
    private readonly provinces: TypedMap<Province> = { }

    all() {
        return Object.values(this.provinces)
    }

    get(name: string) {
        return this.provinces[name]
    }

    create(name: string) {
        const province = new Province(name)
        this.provinces[name] = province
        return province
    }

    getOrCreate(name: string) {
        return this.get(name) ?? this.create(name)
    }
}

export class Level {
    constructor(public readonly index: number, public readonly label: string) {

    }

    readonly regions: Region[] = [ ]
    readonly regionMap: TypedMap<Region> = { }

    add(region: Region) {
        this.regions.push(region)
        this.regionMap[region.key] = region
    }

    get(x: number, y: number) {
        return this.regionMap[`${x} ${y} ${this.index}`]
    }
}

export interface Levels {
    [ level: number ]: Level
}

export interface WorldIndfo {
    width: number
    height: number
}

export class World {
    constructor(public readonly info: WorldIndfo, public readonly ruleset: Ruleset) {

    }

    readonly levels: Levels = { }
    readonly provinces = new Provinces()

    addRegions(regions: RegionFragment[]) {
        for (const reg of regions) {
            this.addRegion(reg)
        }
    }

    addRegion(region: RegionFragment) {
        const reg = Region.from(region, this.ruleset)

        const province = this.provinces.getOrCreate(region.province)
        province.add(reg)

        let level: Level = this.levels[reg.coords.z]
        if (!level) {
            level = new Level(reg.coords.z, reg.coords.label)
            this.levels[reg.coords.z] = level
        }

        level.add(reg)

        return reg
    }

    getRegion(x: number, y: number, z: number) {
        const level = this.levels[z]
        if (!level) return null

        return level.get(x, y)
    }
}
