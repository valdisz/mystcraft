import { RegionFragment, StructureFragment, UnitFragment } from '../../schema';
import { Ruleset } from "./ruleset";
import { Region } from "./region";
import { Level } from "./level";
import { WorldInfo } from "./world-info";
import { Provinces } from './province';
import { Factions, Unit } from './types';
import { Structure } from './structure';

export class World {
    constructor(public readonly info: WorldInfo, public readonly ruleset: Ruleset) {
        for (let i = 0; i < info.map.length; i++) {
            var { width, label } = info.map[i]
            this.addLevel(width, i, label)
        }
    }

    readonly levels: Level[] = [];
    readonly provinces = new Provinces();
    readonly factions = new Factions();

    addFaction(num: number, name: string, isPlayer: boolean) {
        this.factions.create(num, name, isPlayer)
    }

    addRegions(regions: RegionFragment[]) {
        for (const reg of regions) {
            this.addRegion(reg);

            for (const str of reg.structures) {
                this.addStructure(str);
            }
        }

        // exits
        for (const reg of regions) {
            var source = this.getRegionByCode(reg.code)

            for (const exit of reg.exits) {
                const target = this.getRegionByCode(exit.targetRegion)

                source.neighbors.set(source, exit.direction, target)
            }
        }

        // neighbors
        for (const province of this.provinces.all()) {
            const neighbors = new Set<string>()
            for (const reg of province.regions) {
                for (const exit of reg.neighbors.all()) {
                    neighbors.add(exit.target.province.name)
                }
            }

            for (const name of neighbors) {
                if (name === province.name) continue

                const other = this.provinces.get(name)
                province.addBorderWith(other)
            }
        }
    }

    addUnits(units: UnitFragment[]) {
        for (const unit of units) {
            this.addUnit(unit);
        }
    }

    addRegion(region: RegionFragment) {
        const reg = Region.from(region, this.ruleset);

        const province = this.provinces.getOrCreate(region.province);
        province.add(reg);

        let level: Level = this.getLevel(reg.coords.z);
        level.add(reg);

        return reg;
    }

    addStructure(structure: StructureFragment) {
        const str = Structure.from(structure, this.ruleset)
        const region = this.getRegionByCode(structure.regionCode)

        region.addStructure(str)
    }

    addUnit(unit: UnitFragment) {
        const u = Unit.from(unit, this.factions, this.ruleset)
        const region = this.getRegionByCode(unit.regionCode)
        const structure = unit.structureNumber ? region.structures.find(x => x.num === unit.structureNumber) : null

        region.addUnit(u, structure)
    }

    getRegion(x: number, y: number, z: number) {
        const level = this.getLevel(z);
        if (!level) return null;

        return level.get(x, y);
    }

    getRegionById(id: string) {
        for (let i = 0; i < this.levels.length; i++) {
            const level = this.levels[i]
            const region = level.getById(id)
            if (region) return region
        }

        return null
    }

    getRegionByCode(code: string) {
        for (let i = 0; i < this.levels.length; i++) {
            const level = this.levels[i]
            const region = level.getByCode(code)
            if (region) return region
        }

        return null
    }

    getLevel(z: number) {
        return this.levels[z]
    }

    private addLevel(width: number, z: number, label: string) {
        const level = new Level(width, z, label)
        this.levels.push(level)

        return level
    }
}
