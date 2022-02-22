import { Stance } from '../schema'
import { Troops } from './internal'

export class Faction {
    constructor(
        public readonly num: number,
        public readonly name: string,
        public readonly isPlayer: boolean) {
            this.known = num > 0
        }

    readonly troops: Troops = new Troops()

    readonly known: boolean

    stance: Stance = Stance.Neutral
}

export class Factions {
    constructor() {
        this.unknown = this.create(0, '', false)
    }

    private readonly factions: Map<number,Faction> = new Map()

    readonly unknown: Faction
    player: Faction

    all() {
        return this.factions.values()
    }

    get(num: number) {
        return this.factions.get(num)
    }

    create(num: number, name: string, isPlayer: boolean) {
        if (this.factions.has(num)) {
            return this.get(num)
        }

        const faction = new Faction(num, name, isPlayer)
        this.factions.set(num, faction)

        if (isPlayer) {
            this.player = faction
        }

        return faction
    }
}
