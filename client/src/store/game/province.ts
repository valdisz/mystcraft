import { TypedMap } from './typed-map';
import { Region } from './types';


export class Province {
    constructor(public readonly name: string) {
    }

    readonly regions: Region[] = [];

    add(region: Region) {
        this.regions.push(region);
        region.province = this;
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
