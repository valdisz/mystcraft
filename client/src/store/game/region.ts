import { Direction, RegionFragment } from '../../schema'
import { Item } from './item'
import { ItemMap } from './item-map'
import { Province } from './province'
import { Coords } from "./coords"
import { TerrainInfo } from "./terrain-info"
import { Settlement } from "./settlement"
import { Ruleset } from "./ruleset"
import { Wages } from "./wages"
import { Structure, Unit, Troops } from './types'
import { SettlementSize } from './settlement-size'
import { TypedMap } from './typed-map'
import { Links } from './link'

export class Region {
    constructor(public readonly id: string, public readonly coords: Coords) {
        this.code = `${coords.x},${coords.y},${coords.z}`
    }

    readonly code: string

    explored: boolean
    lastVisitedAt: number

    get isVisible() {
        return this.units.length > 0 || this.structures.some(x => x.units.length > 0)
    }

    province: Province
    readonly neighbors = new Links()

    terrain: TerrainInfo
    population: Item | null
    settlement: Settlement | null
    tax: number
    wages: Wages
    entertainment: number
    readonly wanted: ItemMap<Item> = new ItemMap()
    readonly forSale: ItemMap<Item> = new ItemMap()
    readonly products: ItemMap<Item> = new ItemMap()

    readonly units: Unit[] = []
    readonly troops = new Map<number, Troops>()
    readonly structures: Structure[] = []

    addUnit(unit: Unit, structure?: Structure) {
        unit.region = this
        this.units.push(unit)

        if (structure) {
            unit.structure = structure
            structure.units.push(unit)
        }

        const faction = unit.faction

        if (!this.troops.has(faction.num)) {
            this.troops.set(faction.num, new Troops(faction))
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

    static from(src: RegionFragment, ruleset: Ruleset) {
        const reg = new Region(src.id, new Coords(src.x, src.y, src.z, src.label));

        reg.explored = src.explored
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

    toString() {
        return `${this.terrain.name} (${this.coords}) in ${this.province.name}`;
    }
}
