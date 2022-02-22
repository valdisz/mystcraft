import { ExtendedMap, Unit } from './internal'

export class Troops extends ExtendedMap<number, Unit> {
    constructor() {
        super(unit => unit.num)
    }

    has(unit: Unit | number) {
        return super.has(typeof unit === 'number' ? unit : unit.num)
    }

    delete(unit: Unit | number) {
        return super.delete(typeof unit === 'number' ? unit : unit.num)
    }
}
