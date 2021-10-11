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
        }
    }

    addStructures(structures: StructureFragment[]) {
        for (const str of structures) {
            this.addStructure(str);
        }
    }

    addUnits(units: UnitFragment[]) {
        for (const unit of units) {
            this.addUnit(unit);
        }
    }

    addRegion(region: RegionFragment) {
        const reg = Region.from(region, this.factions, this.ruleset);

        const province = this.provinces.getOrCreate(region.province);
        province.add(reg);

        let level: Level = this.getLevel(reg.coords.z);
        if (!level) {
            level = this.addLevel(reg.coords.z, reg.coords.label);
        }

        level.add(reg);

        return reg;
    }

    addStructure(structure: StructureFragment) {
        const str = Structure.from(structure, this.ruleset)
        const region = this.getRegionById(structure.regionId)

        region.addStructure(str)
    }

    addUnit(unit: UnitFragment) {
        const u = Unit.from(unit, this.factions, this.ruleset)
        const region = this.getRegionById(unit.regionId)
        const structure = unit.structureId ? region.structures.find(x => x.id === unit.structureId) : null

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

    getLevel(z: number) {
        for (let i = 0; i < this.levels.length; i++) {
            const level = this.levels[i]
            if (level.index === z) return level
        }

        return null
    }

    addLevel(z: number, label: string) {
        const level = new Level(z, label)
        this.levels.push(level)

        return level
    }
}
