import { Region } from './region';
import { Unit } from "./unit";


export class Structure {
    constructor(public readonly region: Region) {
    }

    readonly units: Unit[] = [];
}
