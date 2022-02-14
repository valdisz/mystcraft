import { Region } from './region'
import { ICoords } from './coords'

export class Level implements Iterable<Region> {
    constructor(public readonly width: number, public readonly height: number, public readonly index: number, public readonly label: string) {
        this.regions = new Array(Math.floor(this.width * this.height / 2) + 1)
    }

    private readonly regions: Region[]
    private readonly regionIdMap = new Map<string, Region>()

    private getIndex(x: number, y: number) {
        return x + y * this.width / 2
    }

    private *getRegions() {
        for (const reg of this.regions) {
            if (reg) yield reg
        }
    }

    [Symbol.iterator](): Iterator<Region, any, undefined> {
        return this.getRegions()
    }

    add(region: Region) {
        this.regions[this.getIndex(region.coords.x, region.coords.y)] = region

        if (region.id) {
            this.regionIdMap.set(region.id, region)
        }
    }

    get(coords: ICoords)
    get(x: number, y: number)
    get(coordsOrX: number | ICoords, y?: number) {
        const index = typeof coordsOrX === 'number'
            ? this.getIndex(coordsOrX, y)
            : this.getIndex(coordsOrX.x, coordsOrX.y)

        return index >= 0 ? this.regions[index] : null
    }

    getById(id: string) {
        return this.regionIdMap.get(id)
    }
}
