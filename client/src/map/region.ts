// import { Container, Graphics, Point, Polygon } from 'pixi.js'

// export class MapRegion extends Container {
//     constructor(public readonly p: Point, public readonly terrain: string, scale: number = 4) {
//         super();

//         const w = 14 * scale;
//         const h = 12 * scale;

//         this.sizeW = w;
//         this.sizeH = h;

//         const hex = new Polygon([
//             w, h / 2,
//             w / 4 * 3, h,
//             w / 4, h,
//             0, h / 2,
//             w / 4, 0,
//             w / 4 * 3, 0
//         ]);

//         const terrainMap = {
//             'plain': 0xa2a552,
//             'forest': 0x2a603b,
//             'hill': 0xc9c832,
//             'mountain': 0xf7f9fb,
//             'lake': 0x86aba5,
//             'swamp': 0x52593b,
//             'tundra': 0x8c9e5e,
//             'jungle': 0x7a942e,
//             'desert': 0xedc9af,
//             'wasteland': 0x63424b,

//             'ocean': 0x2654a1,
//             'shallows': 0x54a5d5,
//             'deeps': 0x243274,

//             'cavern': 0xf0f0f0,
//             'chasm': 0xf0f0f0,
//             'deepforest': 0xf0f0f0,
//             'grotto': 0xf0f0f0,
//             'mystforest': 0xf0f0f0,
//             'tunnels': 0xf0f0f0,
//             'underforest': 0xf0f0f0,
//         };
//         const color = terrainMap[terrain] || 0xf0f0f0;

//         const g = new Graphics();
//         g.beginFill(color);
//         g.drawPolygon(hex);
//         g.endFill();

//         this.addChild(g);

//         const dX = 0;
//         const dY = 0;

//         this.x = (pX - 1) * (this.width / 4 * 3) - dX * pX;
//         this.y = (pY - 1) * (this.height / 2) - dY * pY;
//         this.zIndex = pY;
//     }

//     readonly sizeW;
//     readonly sizeH;
// }
