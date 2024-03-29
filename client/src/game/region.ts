import { Direction, ExitFragment, RegionFragment } from '../schema'
import {
    Item, ItemMap, Province, Coords, TerrainInfo, Settlement, Ruleset, Wages, Structure, SettlementSize, Links, Unit, Troops, Battle, TradeRoute
} from './internal'

export class Region {
    constructor(public readonly id: string, public readonly coords: Coords) {
    }

    explored: boolean

    /** Is region information about the region hidden or not */
    covered: boolean
    lastVisitedAt: number

    get isVisible() {
        return this.units.length > 0
    }

    province: Province
    readonly neighbors = new Links()
    readonly incomingTrade: TradeRoute[] = [ ]
    readonly outgoingTrade: TradeRoute[] = [ ]

    terrain: TerrainInfo
    population: Item | null
    settlement: Settlement | null
    tax: number
    wages: Wages
    entertainment: number
    gate: number | null
    readonly wanted: ItemMap<Item> = new ItemMap()
    readonly forSale: ItemMap<Item> = new ItemMap()
    readonly products: ItemMap<Item> = new ItemMap()

    readonly units: Unit[] = []
    readonly troops = new Map<number, Troops>()
    readonly structures: Structure[] = []
    readonly battles: Battle[] = []

    get onGuard() {
        return this.units.filter(x => x.onGuard)
    }

    addUnit(unit: Unit, structure?: Structure) {
        unit.region = this
        this.units.push(unit)

        if (structure) {
            unit.structure = structure
            structure.units.push(unit)
        }

        const faction = unit.faction

        if (!this.troops.has(faction.num)) {
            this.troops.set(faction.num, new Troops())
        }

        this.troops.get(faction.num).add(unit)
    }

    addStructure(str: Structure) {
        str.region = this
        this.structures.push(str)
    }

    sort() {
        this.structures.sort((a, b) => a.seq - b.seq)
        this.units.sort((a, b) => a.seq - b.seq)

        for (const str of this.structures) {
            str.units.sort((a, b) => a.seq - b.seq)
        }
    }

    addTradeRoute(route: TradeRoute) {
        // if (route.distance > 24) {
        //     return
        // }

        if (route.buy.region === this) {
            this.outgoingTrade.push(route)
            this.outgoingTrade.sort((a, b) => a.distance - b.distance)
        }

        if (route.sell.region === this) {
            this.incomingTrade.push(route)
            this.incomingTrade.sort((a, b) => a.distance - b.distance)
        }
    }

    toString() {
        let s = `${this.terrain.name} (${this.coords}) in ${this.province.name}`;
        if (this.settlement) {
            s += ` contains ${this.settlement.name} ${this.settlement.size.toLowerCase()}`
        }

        return s
    }

    static from(src: RegionFragment, ruleset: Ruleset) {
        const reg = new Region(src.id, new Coords(src.x, src.y, src.z, src.label));

        reg.explored = src.explored
        reg.covered = false
        reg.lastVisitedAt = src.lastVisitedAt

        if (src.race) {
            reg.population = ruleset.getItem(src.race).create(src.population ?? 0)
        }

        if (src.settlement) {
            reg.settlement = {
                name: src.settlement.name,
                size: src.settlement.size.toLowerCase() as SettlementSize
            }
        }

        reg.terrain = ruleset.getTerrain(src.terrain);

        reg.tax = src.tax;
        reg.wages = {
            amount: src.wages,
            total: src.totalWages
        };
        reg.entertainment = src.entertainment;

        reg.gate = src.gate

        for (const prod of src.produces) {
            const item = ruleset.getItem(prod.code).create();
            item.amount = prod.amount;

            reg.products.set(item);
        }

        for (const sale of src.forSale) {
            const item = ruleset.getItem(sale.code).create();
            item.amount = sale.amount;
            item.price = sale.price;

            reg.forSale.set(item);
        }

        for (const wanted of src.wanted) {
            const item = ruleset.getItem(wanted.code).create();
            item.amount = wanted.amount;
            item.price = wanted.price;

            reg.wanted.set(item);
        }

        return reg;
    }

    static fromExit({ x, y, z, label, terrain, settlement }: ExitFragment, ruleset: Ruleset) {
        const reg = new Region(null, new Coords(x, y, z, label))

        reg.explored = false
        reg.covered = false
        reg.lastVisitedAt = 0

        if (settlement) {
            reg.settlement = {
                name: settlement.name,
                size: settlement.size.toLowerCase() as SettlementSize
            }
        }

        reg.terrain = ruleset.getTerrain(terrain)

        return reg
    }

    static createCovered(x: number, y: number, z: number, label: string, ruleset: Ruleset) {
        const reg = new Region(null, new Coords(x, y, z, label))

        reg.explored = false
        reg.covered = true
        reg.lastVisitedAt = 0
        reg.terrain = ruleset.getTerrain(TerrainInfo.UNKNOWN)

        return reg
    }
}
