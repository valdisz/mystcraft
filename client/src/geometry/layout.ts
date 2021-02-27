import { Point } from 'pixi.js';
import { DoubledCoord } from './doubled-coord';
import { Hex } from './hex';
import { Orientation } from './orientation';

export class Layout {
    constructor(
        public orientation: Orientation,
        public size: Point,
        public origin: Point
    ) { }

    public hexToPixel(h: Hex | DoubledCoord): Point {
        if (h.type === 'double') {
            return this.hexToPixel(h.toCube())
        }

        const M: Orientation = this.orientation;
        const size: Point = this.size;
        const origin: Point = this.origin;

        const x = (M.f0 * h.q + M.f1 * h.r) * size.x;
        const y = (M.f2 * h.q + M.f3 * h.r) * size.y;

        return new Point(x + origin.x, y + origin.y);
    }

    public pixelToHex(p: Point): Hex {
        const M: Orientation = this.orientation;

        const size: Point = this.size;
        const origin: Point = this.origin;
        const pt: Point = new Point(
            (p.x - origin.x) / size.x,
            (p.y - origin.y) / size.y
        );

        let q: number = M.b0 * pt.x + M.b1 * pt.y;
        const r: number = M.b2 * pt.x + M.b3 * pt.y;

        let hex = new Hex(q, r, -q - r).round();

        return hex;
    }

    public hexCornerOffset(corner: number): Point {
        var M: Orientation = this.orientation;
        var size: Point = this.size;
        var angle: number = (2.0 * Math.PI * (M.start_angle - corner)) / 6.0;
        return new Point(size.x * Math.cos(angle), size.y * Math.sin(angle));
    }

    public polygonCorners(h: Hex): Point[] {
        var corners: Point[] = [];
        var center: Point = this.hexToPixel(h);
        for (var i = 0; i < 6; i++) {
            var offset: Point = this.hexCornerOffset(i);
            corners.push(new Point(center.x + offset.x, center.y + offset.y));
        }
        return corners;
    }
}
