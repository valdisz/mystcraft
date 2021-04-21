import { StructureFragment } from "../../schema";
import { Ruleset } from "./ruleset";
import { Region, Unit } from "./types";

export class Structure {
    constructor(public readonly region: Region) {
    }

    id: string
    num: number
    name: string
    description: string
    type: string

    readonly units: Unit[] = [];

    static from(region: Region, src: StructureFragment, ruleset: Ruleset): Structure {
        const struct = new Structure(region)

        struct.id = src.id
        struct.num = src.number
        struct.name = src.name
        struct.description = src.description
        struct.type = src.type

        return struct
    }
}
