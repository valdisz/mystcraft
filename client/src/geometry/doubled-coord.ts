import { Hex } from './hex'

export class Coord {
    constructor(public x: number, public y: number) { }

    readonly type = 'double'

    public static fromCube(h: Hex): Coord {
        const col = h.q
        const row = 2 * h.r + h.q

        return new Coord(col, row)
    }

    public toCube(): Hex {
        const q = this.x
        const r = (this.y - this.x) / 2
        const s = -q - r

        return new Hex(q, r, s)
    }
}
