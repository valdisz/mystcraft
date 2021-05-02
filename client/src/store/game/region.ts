import { RegionFragment } from '../../schema'
import { Item } from './item'
import { List } from './list'
import { Province } from './province'
import { Coords } from "./coords"
import { TerrainInfo } from "./terrain-info"
import { Settlement } from "./settlement"
import { Population } from "./population"
import { Ruleset } from "./ruleset"
import { Wages } from "./wages"
import { Structure, Unit, Troops, Factions } from './types'
import { SettlementSize } from './settlement-size'
import { TypedMap } from './typed-map'

export class Region {
    constructor(public readonly id: string, public readonly coords: Coords) {
        this.key = `${coords.x} ${coords.y} ${coords.z}`
    }

    readonly key: string

    explored: boolean
    updatedAtTurn: number

    get isVisible() {
        return this.units.length > 0 || this.structures.some(x => x.units.length > 0)
    }

    province: Province
    terrain: TerrainInfo
    population: Item | null
    settlement: Settlement | null
    tax: number
    wages: Wages
    entertainment: number
    readonly wanted: List<Item> = new List()
    readonly forSale: List<Item> = new List()
    readonly products: List<Item> = new List()

    readonly units: Unit[] = []
    readonly troops: TypedMap<Troops> = {}
    readonly structures: Structure[] = []

    static from(src: RegionFragment, factions: Factions, ruleset: Ruleset) {
        const reg = new Region(src.id, new Coords(src.x, src.y, src.z, src.label));

        reg.explored = src.explored
        reg.updatedAtTurn = src.updatedAtTurn

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

        for (const prod of src.products) {
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

        src.units.sort((a, b) => a.sequence - b.sequence)

        for (const unitSource of src.units) {
            const unit = Unit.from(unitSource, ruleset)

            unit.region = reg
            reg.units.push(unit)

            const faction = unitSource.faction
                ? factions.get(unitSource.faction.number)
                : factions.unknown

            unit.faction = faction
            faction.troops.add(unit)

            if (!reg.troops[faction.num]) {
                reg.troops[faction.num] = new Troops(faction)
            }
            reg.troops[faction.num].add(unit)
        }

        for (const structSrc of src.structures) {
            const str = Structure.from(reg, structSrc, ruleset)
            reg.structures.push(str)

            for (const unitSource of structSrc.units) {
                const unit = Unit.from(unitSource, ruleset)

                unit.region = reg
                unit.structure = str

                reg.units.push(unit)
                str.units.push(unit)

                const faction = unitSource.faction
                    ? factions.get(unitSource.faction.number)
                    : factions.unknown

                unit.faction = faction
                faction.troops.add(unit)

                if (!reg.troops[faction.num]) {
                    reg.troops[faction.num] = new Troops(faction)
                }
                reg.troops[faction.num].add(unit)
            }
        }

        reg.structures.sort((a, b) => a.num - b.num)

        return reg;
    }
}
