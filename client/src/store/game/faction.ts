import { TypedMap } from "./typed-map";
import { Troops } from "./types";

export class Faction {
    constructor(
        public readonly num: number,
        public readonly name: string,
        public readonly isPlayer: boolean) {

        }

    readonly troops: Troops = new Troops(this)
}

export class Factions {
    constructor() {
        this.unknown = this.create(-1, '', false)
    }

    private readonly factions: TypedMap<Faction> = { }

    readonly unknown: Faction

    all() {
        return Object.values(this.factions)
    }

    get(num: number) {
        return this.factions[num]
    }

    create(num: number, name: string, isPlayer: boolean) {
        const faction = new Faction(num, name, isPlayer)
        this.factions[num] = faction
        return faction
    }
}
