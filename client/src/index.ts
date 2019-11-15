import { Application, Container, Graphics, Sprite, ITextureDictionary, Point as PixiPoint, LineStyle } from 'pixi.js';
import { Hex, Layout, Point, OffsetCoord, Orientation, DoubledCoord } from './map';

const app = new Application({
    antialias: true,
    transparent: true,
    resolution: 1
});
const { loader, renderer, stage } = app;
const { resources } = loader;

renderer.autoResize = true;
renderer.resize(window.innerWidth, window.innerHeight);
document.getElementById('host').appendChild(app.view);

interface MapHex {
    hex: Hex;
    terrain: string;
}

function makeHex(col: number, row: number, terrain: string): MapHex {
    const hex = new DoubledCoord(col, row).toCube();
    return {
        hex,
        terrain: terrain + '.png'
    };
}

const layout = new Layout(
    Layout.flat,
    new Point(16, -16),
    new Point(16, 32)
);

const map = [
    makeHex(32, 16, 'ocean'),
    makeHex(32, 18, 'ocean'),
    makeHex(32, 20, 'ocean'),
    makeHex(32, 22, 'ocean'),

    makeHex(33, 11, 'ocean'),
    makeHex(33, 13, 'ocean'),
    makeHex(33, 15, 'ocean'),

    makeHex(34, 10, 'ocean'),

    makeHex(35, 9, 'ocean'),
    makeHex(35, 21, 'ocean'),
    makeHex(35, 23, 'ocean'),

    makeHex(36, 10, 'ocean'),
    makeHex(36, 16, 'ocean'),
    makeHex(36, 20, 'ocean'),

    makeHex(37, 11, 'ocean'),
    makeHex(37, 13, 'ocean'),
    makeHex(37, 15, 'ocean'),
    makeHex(37, 17, 'ocean'),
    makeHex(37, 19, 'ocean'),

    makeHex(35, 11, 'plain'),
    makeHex(34, 12, 'plain'),
    makeHex(36, 12, 'swamp'),

    makeHex(35, 13, 'plain'),
    makeHex(34, 14, 'plain'),
    makeHex(36, 14, 'swamp'),

    makeHex(35, 15, 'plain'),
    makeHex(34, 16, 'plain'),

    makeHex(33, 17, 'mountain'),
    makeHex(35, 17, 'plain'),

    makeHex(34, 18, 'mountain'),
    makeHex(36, 18, 'mountain'),
    makeHex(33, 19, 'mountain'),
    makeHex(35, 19, 'mountain'),

    makeHex(34, 20, 'mountain'),
    makeHex(33, 21, 'mountain'),

    makeHex(34, 22, 'forest'),
    makeHex(33, 23, 'mountain'),

    makeHex(34, 24, 'mountain')
];

function drawHex({ hex, terrain }: MapHex, tileset: ITextureDictionary): Container {
    const sprite = new Sprite(tileset[terrain]);

    const container = new Container();
    container.addChild(sprite);

    const { row, col } = DoubledCoord.fromCube(hex);
    container.x = col * 24;
    container.y = row * 14;

    return container;
}

function resizeRendererOnWindowResize() {
    window.onresize = () => {
        renderer.resize(window.innerWidth, window.innerHeight);
    };
}

function resourcesLoaded() {
    const scale = 1;
    const tileset = resources['textures/terrain.json'].textures;
    const mapLayer = new Container();
    mapLayer.scale = new PixiPoint(scale, scale);

    const srotedMap = map.sort((a, b) => {
        const { col: aCol, row: aRow } = DoubledCoord.fromCube(a.hex);
        const { col: bCol, row: bRow } = DoubledCoord.fromCube(b.hex);

        if (aRow > bRow) return 1;
        if (aRow < bRow) return -1;

        if (aCol > bCol) return 1;
        if (aCol < bCol) return -1;

        return 0;
    })

    for (let i = 0; i < srotedMap.length; i++) {
        const tile = drawHex(srotedMap[i], tileset);
        mapLayer.addChild(tile);
    }

    mapLayer.x = -Math.min(...mapLayer.children.map(x => x.x)) * scale + 100;
    mapLayer.y = -Math.min(...mapLayer.children.map(x => x.y)) * scale + 100;

    stage.addChild(mapLayer);

    app.ticker.stop();
    app.ticker.update();
}

function start() {
    loader
        .add('textures/terrain.json')
        .load(resourcesLoaded);
}

resizeRendererOnWindowResize();
start();
