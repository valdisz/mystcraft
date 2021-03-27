import { RegionFragment } from '../../schema';
import { Ruleset } from "./ruleset";
import { Region } from "./region";
import { Level, Levels } from "./level";
import { WorldInfo } from "./world-info";
import { Provinces } from './province';
import { Factions } from './types';

export class World {
    constructor(public readonly info: WorldInfo, public readonly ruleset: Ruleset) {
    }

    readonly levels: Levels = {};
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

    addRegion(region: RegionFragment) {
        const reg = Region.from(region, this.factions, this.ruleset);

        // for (const unit of region.units ?? []) {
        //     const u = Unit.from(unit, this.ruleset)
        //     reg.units.push(u)
        // }

        const province = this.provinces.getOrCreate(region.province);
        province.add(reg);

        let level: Level = this.levels[reg.coords.z];
        if (!level) {
            level = new Level(reg.coords.z, reg.coords.label);
            this.levels[reg.coords.z] = level;
        }

        level.add(reg);

        return reg;
    }

    getRegion(x: number, y: number, z: number) {
        const level = this.levels[z];
        if (!level)
            return null;

        return level.get(x, y);
    }
}
