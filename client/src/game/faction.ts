import { Stance } from '../schema'
import { ExtendedMap, Troops } from './internal'

export class Faction {
    constructor(
        public readonly num: number,
        public readonly name: string,
        public readonly isPlayer: boolean) {
            this.known = num > 0
        }

    readonly troops: Troops = new Troops()

    readonly known: boolean

    attitude: Stance = Stance.Neutral
}

export class Factions extends ExtendedMap<number, Faction> {
    constructor() {
        super(x => x.num)

        this.unknown = this.create(0, '', false)
    }

    readonly unknown: Faction
    player: Faction

    create(num: number, name: string, isPlayer: boolean) {
        if (this.has(num)) {
            return this.get(num)
        }

        const faction = new Faction(num, name, isPlayer)
        this.set(num, faction)

        if (isPlayer) {
            this.player = faction
        }

        return faction
    }
}
