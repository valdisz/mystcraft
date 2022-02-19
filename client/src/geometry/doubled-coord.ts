import { Hex } from './hex'

export interface ICoord {
    x: number
    y: number
}

export class Coord implements ICoord {
    constructor(public x: number, public y: number) { }

    readonly type = 'double'

    public static fromCube(h: Hex): Coord {
        const col = h.q
        const row = 2 * h.r + h.q

        return new Coord(col, row)
    }

    public static toCube({ x, y }: ICoord): Hex {
        const q = x
        const r = (y - x) / 2
        const s = -q - r

        return new Hex(q, r, s)
    }

    public toCube(): Hex {
        return Coord.toCube(this)
    }
}
