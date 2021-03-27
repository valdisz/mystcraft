import { Region, Unit } from "./types";

export class Structure {
    constructor(public readonly region: Region) {
    }

    readonly units: Unit[] = [];
}
