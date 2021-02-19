import { Hex } from './hex';

export class OffsetCoord {
    constructor(public col: number, public row: number) { }
    public static EVEN: number = 1;
    public static ODD: number = -1;

    public static qoffsetFromCube(offset: number, h: Hex): OffsetCoord {
        var col: number = h.q;
        var row: number = h.r + (h.q + offset * (h.q & 1)) / 2;
        if (offset !== OffsetCoord.EVEN && offset !== OffsetCoord.ODD) {
            throw 'offset must be EVEN (+1) or ODD (-1)';
        }
        return new OffsetCoord(col, row);
    }

    public static qoffsetToCube(offset: number, h: OffsetCoord): Hex {
        var q: number = h.col;
        var r: number = h.row - (h.col + offset * (h.col & 1)) / 2;
        var s: number = -q - r;
        if (offset !== OffsetCoord.EVEN && offset !== OffsetCoord.ODD) {
            throw 'offset must be EVEN (+1) or ODD (-1)';
        }
        return new Hex(q, r, s);
    }

    public static roffsetFromCube(offset: number, h: Hex): OffsetCoord {
        var col: number = h.q + (h.r + offset * (h.r & 1)) / 2;
        var row: number = h.r;
        if (offset !== OffsetCoord.EVEN && offset !== OffsetCoord.ODD) {
            throw 'offset must be EVEN (+1) or ODD (-1)';
        }
        return new OffsetCoord(col, row);
    }

    public static roffsetToCube(offset: number, h: OffsetCoord): Hex {
        var q: number = h.col - (h.row + offset * (h.row & 1)) / 2;
        var r: number = h.row;
        var s: number = -q - r;
        if (offset !== OffsetCoord.EVEN && offset !== OffsetCoord.ODD) {
            throw 'offset must be EVEN (+1) or ODD (-1)';
        }
        return new Hex(q, r, s);
    }
}
