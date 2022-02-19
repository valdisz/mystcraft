import { Point, IPointData } from 'pixi.js'
import { Coord, ICoord } from './doubled-coord'
import { Hex } from './hex'
import { Orientation } from './orientation'

export class Layout {
    constructor(size: IPointData, origin: IPointData) {
        this.size = new Point(size.x, size.y)
        this.origin = new Point(origin.x, origin.y)
    }

    public readonly orientation = Orientation.FLAT
    public readonly size: Point
    public readonly origin: Point

    public toPixel(h: Hex | ICoord): IPointData {
        if (!(h instanceof Hex)) {
            return this.toPixel(Coord.toCube(h))
        }

        const M = this.orientation
        const x = (M.f0 * h.q + M.f1 * h.r) * this.size.x
        const y = (M.f2 * h.q + M.f3 * h.r) * this.size.y

        return { x: x + this.origin.x, y: y + this.origin.y }
    }

    public toHex(p: IPointData): Hex {
        const x = (p.x - this.origin.x) / this.size.x
        const y = (p.y - this.origin.y) / this.size.y

        const M = this.orientation
        const q = M.b0 * x + M.b1 * y
        const r = M.b2 * x + M.b3 * y

        const hex = new Hex(q, r, -q - r).round()
        return hex;
    }

    public toCoord(p: IPointData): Coord {
        return Coord.fromCube(this.toHex(p))
    }

    public cornerOffset(corner: number): IPointData {
        const M = this.orientation
        const angle = (2.0 * Math.PI * (M.start_angle - corner)) / 6.0
        return { x: this.size.x * Math.cos(angle), y: this.size.y * Math.sin(angle) }
    }

    public corners(h: Hex): IPointData[] {
        const corners: IPointData[] = []

        const center = this.toPixel(h)
        for (let i = 0; i < 6; i++) {
            const offset = this.cornerOffset(i)
            corners.push({x: center.x + offset.x, y: center.y + offset.y })
        }

        return corners
    }
}
