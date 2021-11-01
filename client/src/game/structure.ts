import { StructureFragment } from "../schema";
import { Ruleset } from "./ruleset";
import { Region, Unit } from "./types";

export class Structure {
    constructor() {
    }

    id: string
    num: number
    seq: number
    name: string
    description: string
    type: string

    region: Region
    readonly units: Unit[] = [];

    static from(src: StructureFragment, ruleset: Ruleset): Structure {
        const struct = new Structure()

        struct.id = src.id
        struct.num = src.number
        struct.seq = src.sequence
        struct.name = src.name
        struct.description = src.description
        struct.type = src.type

        return struct
    }
}
