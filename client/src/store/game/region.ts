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

export class Region {
    constructor(public readonly id: string, public readonly coords: Coords) {
        this.key = `${coords.x} ${coords.y} ${coords.z}`;
    }

    readonly key: string;

    province: Province;
    terrain: TerrainInfo;
    population: Population | null;
    settlement: Settlement | null;
    tax: number;
    wages: Wages;
    entertainment: number;
    readonly wanted: List<Item> = new List();
    readonly forSale: List<Item> = new List();
    readonly products: List<Item> = new List();

    readonly units: Unit[] = [];
    // readonly troops: Troops[] = [];
    readonly structures: Structure[] = [];

    static from(src: RegionFragment, factions: Factions, ruleset: Ruleset) {
        const reg = new Region(src.id, {
            x: src.x,
            y: src.y,
            z: src.z,
            label: src.label
        });

        reg.population = {
            amount: src.population,
            race: ruleset.getItem(src.race)
        };

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

            unit.faction = unitSource.faction
                ? factions.get(unitSource.faction.number)
                : null
        }

        for (const struct of src.structures) {
            for (const unitSource of struct.units) {
                const unit = Unit.from(unitSource, ruleset)

                unit.region = reg
                reg.units.push(unit)

                unit.faction = unitSource.faction
                    ? factions.get(unitSource.faction.number)
                    : null
            }
        }

        return reg;
    }
}
