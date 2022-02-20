import { ExtendedMap } from './extended-map';
import { Faction, Unit } from './types'

export class Troops extends ExtendedMap<number, Unit> {
    constructor(public readonly faction: Faction | null) {
        super(unit => unit.num)
    }

    has(unit: Unit | number) {
        return super.has(typeof unit === 'number' ? unit : unit.num)
    }

    delete(unit: Unit | number) {
        return super.delete(typeof unit === 'number' ? unit : unit.num)
    }
}
