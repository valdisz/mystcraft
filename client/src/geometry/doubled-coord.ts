import { Hex } from './hex';

export class DoubledCoord {
    constructor(public col: number, public row: number) { }

    public static qdoubledFromCube(h: Hex): DoubledCoord {
        var col: number = h.q;
        var row: number = 2 * h.r + h.q;
        return new DoubledCoord(col, row);
    }

    public qdoubledToCube(): Hex {
        var q: number = this.col;
        var r: number = (this.row - this.col) / 2;
        var s: number = -q - r;
        return new Hex(q, r, s);
    }

    public static rdoubledFromCube(h: Hex): DoubledCoord {
        var col: number = 2 * h.q + h.r;
        var row: number = h.r;
        return new DoubledCoord(col, row);
    }

    public rdoubledToCube(): Hex {
        var q: number = (this.col - this.row) / 2;
        var r: number = this.row;
        var s: number = -q - r;
        return new Hex(q, r, s);
    }
}
