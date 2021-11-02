import { DoubledCoord, Hex } from "../geometry"

export class Coords extends DoubledCoord {
    constructor(
        x: number,
        y: number,
        public readonly z: number,
        public readonly label?: string) {
            super(x, y)

            this.cube = this.toCube()
        }

    readonly cube: Hex

    toString() {
        const parts = [ this.x, this.y ]
        if (this.z !== 1) parts.push(this.z)

        return parts.join(',')
    }
}
