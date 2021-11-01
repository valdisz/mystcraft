import { TypedMap } from './typed-map';
import { Region } from './types';


export class Province {
    constructor(public readonly name: string) {
    }

    readonly regions: Region[] = [];

    readonly neighbors: Province[] = []

    add(region: Region) {
        this.regions.push(region);
        region.province = this;
    }

    contains(region: Region) {
        for (let i = 0; i < this.regions.length; i++) {
            if (this.regions[i].code === region.code) return true
        }

        return false
    }

    isBorderWith(other: Province) {
        for (let i = 0; i < this.neighbors.length; i++) {
            if (this.neighbors[i].name === other.name) return true
        }

        return false
    }

    addBorderWith(other: Province) {
        if (!this.isBorderWith(other)) this.neighbors.push(other)
        if (!other.isBorderWith(this)) other.neighbors.push(this)
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
