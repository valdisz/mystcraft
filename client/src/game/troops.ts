import { Faction, Unit } from './types'

export class Troops implements Iterable<Unit> {
    constructor(public readonly faction: Faction | null) {
    }

    private readonly units: Map<number, Unit> = new Map();

    [Symbol.iterator](): Iterator<Unit, any, undefined> {
        return this.units.values()
    }

    get all() {
        return Array.from(this.units.values())
    }

    get size() {
        return this.units.size
    }

    add(unit: Unit) {
        this.units.set(unit.num, unit)
    }

    has(unit: Unit | number) {
        return this.units.has(typeof unit === 'number' ? unit : unit.num)
    }

    delete(unit: Unit | number) {
        return this.units.delete(typeof unit === 'number' ? unit : unit.num)
    }

    get(num: number) {
        return this.units.get(num)
    }
}
