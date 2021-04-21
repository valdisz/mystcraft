import { Faction, Unit } from "./types";


export class Troops {
    constructor(public readonly faction: Faction | null) {
    }

    readonly units: Unit[] = []

    add(unit: Unit) {
        this.units.push(unit)
    }
}
