import { Region } from "./region";
import { TypedMap } from "./typed-map";


export class Level {
    constructor(public readonly index: number, public readonly label: string) {
    }

    readonly regions: Region[] = [];
    readonly regionMap: TypedMap<Region> = {};
    readonly regionIdMap: TypedMap<Region> = {};

    add(region: Region) {
        this.regions.push(region);
        this.regionMap[region.key] = region;
        this.regionIdMap[region.id] = region;
    }

    get(x: number, y: number) {
        return this.regionMap[`${x} ${y} ${this.index}`];
    }

    getById(id: string) {
        return this.regionIdMap[id]
    }
}
