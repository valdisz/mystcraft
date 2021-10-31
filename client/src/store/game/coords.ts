import { DoubledCoord, Hex } from "../../geometry"

export class Coords {
    constructor(
        public readonly x: number,
        public readonly y: number,
        public readonly z: number,
        public readonly label?: string) {

    }

    toDouble() {
        return new DoubledCoord(this.x, this.y)
    }

    toCube() {
        return this.toDouble().toCube();
    }

    toString() {
        const parts = [ this.x, this.y ]
        if (this.z !== 1) parts.push(this.z)

        return parts.join(',')
    }
}
