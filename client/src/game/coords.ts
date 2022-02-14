import { Coord, Hex } from '../geometry'

export interface ICoords {
    x: number
    y: number
    z: number
    label?: string
}

export class Coords extends Coord implements ICoords {
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

    equals({ x, y, z}: Coords) {
        return this.x === x && this.y === y && this.z === z
    }
}
