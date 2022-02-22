import { Region } from './internal';

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
            if (this.regions[i].coords.equals(region.coords)) return true
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

export interface ProvincePredicate {
    (province: Province): boolean
}

export interface ProvinceTransformation<T> {
    (province: Province): T
}

export class Provinces implements Iterable<Province> {
    private readonly provinces = new Map<string, Province>();

    [Symbol.iterator](): Iterator<Province, any, undefined> {
        return this.provinces.values()
    }

    toArray(): Province[] {
        return Array.from(this.provinces.values())
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

    some(p: ProvincePredicate): boolean {
        for (const kv of this.provinces) {
            if (p(kv[1])) {
                return true
            }
        }

        return false
    }

    all(p: ProvincePredicate): boolean {
        for (const kv of this.provinces) {
            if (!p(kv[1])) {
                return false
            }
        }

        return true
    }

    map<T>(p: ProvinceTransformation<T>): T[] {
        const items: T[] = [ ]
        for (const kv of this.provinces) {
            items.push(p(kv[1]))
        }

        return items
    }

    find(predicate: ProvincePredicate) {
        const linksIterator = this.provinces.values()
        for (const l of linksIterator) {
            if (predicate(l)) {
                return l
            }
        }

        return null
    }

    has(name: string): boolean {
        return this.provinces.has(name)
    }
}
