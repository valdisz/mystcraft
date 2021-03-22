import { Faction } from "./faction";
import { Unit } from "./unit";


export class Company {
    constructor(public readonly faction: Faction | null, public readonly units: Unit[]) {
    }
}
