import { Hex } from './hex';
import { Orientation } from './orientation';
import { Point } from './point';


export class Layout {
    constructor(
        public orientation: Orientation,
        public size: Point,
        public origin: Point
    ) { }
    public static pointy: Orientation = new Orientation(
        Math.sqrt(3.0),
        Math.sqrt(3.0) / 2.0,
        0.0,
        3.0 / 2.0,
        Math.sqrt(3.0) / 3.0,
        -1.0 / 3.0,
        0.0,
        2.0 / 3.0,
        0.5
    );
    public static flat: Orientation = new Orientation(
        3.0 / 2.0,
        0.0,
        Math.sqrt(3.0) / 2.0,
        Math.sqrt(3.0),
        2.0 / 3.0,
        0.0,
        -1.0 / 3.0,
        Math.sqrt(3.0) / 3.0,
        0.0
    );

    public hexToPixel(h: Hex): Point {
        var M: Orientation = this.orientation;
        var size: Point = this.size;
        var origin: Point = this.origin;
        var x: number = (M.f0 * h.q + M.f1 * h.r) * size.x;
        var y: number = (M.f2 * h.q + M.f3 * h.r) * size.y;
        return new Point(x + origin.x, y + origin.y);
    }

    public pixelToHex(p: Point): Hex {
        var M: Orientation = this.orientation;
        var size: Point = this.size;
        var origin: Point = this.origin;
        var pt: Point = new Point(
            (p.x - origin.x) / size.x,
            (p.y - origin.y) / size.y
        );
        var q: number = M.b0 * pt.x + M.b1 * pt.y;
        var r: number = M.b2 * pt.x + M.b3 * pt.y;
        return new Hex(q, r, -q - r);
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
