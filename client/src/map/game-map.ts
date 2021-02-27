import * as PIXI from 'pixi.js'
import { autoDetectRenderer, Container, Loader, Point, Renderer } from 'pixi.js'
import { Hex, DoubledCoord, Layout, Orientation } from '../geometry'
import { RegionFragment } from '../schema'
import { Viewport } from './viewport'

export interface GetRegionCallback {
    (x: number, y: number): RegionFragment
}

export interface MapSize {
    width: number
    height: number
}

export class Rect {
    constructor(x0: number, y0: number, x1: number, y1: number) {
        this.p0 = new Point(x0, y0)
        this.p1 = new Point(x1, y1)
    }

    p0: Point
    p1: Point

    get width() {
        return this.p1.x - this.p0.x + 1
    }

    get height() {
        return this.p1.y - this.p0.y + 1
    }
}

const TILE_W = 48;
const TILE_H = 48;

export class Tile extends Container {
    constructor(public readonly col: number, public readonly row: number, terrain: PIXI.Texture) {
        super()

        // this.addChild(g)
        this.addChild(new PIXI.Sprite(terrain))
    }
}

export interface TileMap {
    [id: string]: Tile
}

export class GameMap {
    constructor(canvas: HTMLCanvasElement, public readonly size: MapSize, public readonly getRegion: GetRegionCallback) {
        const origin = new Point(TILE_W / 2, TILE_H)

        this.layout = new Layout(
            Orientation.FLAT,
            new Point(TILE_W / 2, TILE_H / 2),
            origin
        )

        const sz = this.layout.hexToPixel(new DoubledCoord(size.width - 1, size.height - 1))
        const maxWidth = sz.x + this.layout.size.x
        const maxHeight = sz.y + this.layout.size.y

        this.viewport = new Viewport(canvas,
            origin,
            maxWidth,
            maxHeight,
            vp => {
                this.layout.origin.x = vp.origin.x
                this.layout.origin.y = vp.origin.y

                this.renderer.resize(vp.width, vp.height)

                this.update()
            }
        )

        this.renderer = autoDetectRenderer({
            width: this.viewport.width,
            height: this.viewport.height,
            view: canvas,
            antialias: true,
            resolution: window.devicePixelRatio || 1
        })

        this.root = new Container()
        this.tiles = new Container()
        this.outline = new Container()
        this.root.addChild(this.outline)
        this.root.addChild(this.tiles)
    }

    readonly loader = new Loader()

    readonly root: Container
    readonly tiles: Container
    readonly outline: Container

    readonly renderer: Renderer
    readonly viewport: Viewport
    readonly layout: Layout
    rect = new Rect(0, 0, 0, 0)

    // tiles: TileMap = { }

    load() {
        return new Promise((resolve, reject) => {
            this.loader.add('/terrain.json');

            this.loader.onError.once(reject)

            this.loader.load((_, res) => resolve(res))
        })
    }

    getTerrainTexture(name: string): PIXI.Texture {
        const sprites = this.loader.resources['/terrain.json'];
        const sheet = sprites.spritesheet
        return sheet.textures[`${name}.png`]
    }

    drawTile(col, row) {
        const dc = new DoubledCoord(col, row)
        const hex = dc.toCube()
        const p = this.layout.hexToPixel(hex)
        p.x -= this.layout.size.x
        p.y -= this.layout.size.y

        col = col % this.size.width
        if (col < 0) col += this.size.width

        const region = this.getRegion(col, row)
        if (region) {
            const tile = new PIXI.Sprite(this.getTerrainTexture(region.terrain))
            tile.position = p
            this.tiles.addChild(tile)

            const g = new PIXI.Graphics()
            g.lineStyle(4, 0x999999)
            g.drawPolygon(this.layout.polygonCorners(hex))
            g.position.set(0, -2.5)
            this.outline.addChild(g)
        }
    }

    update() {
        if (this.loader.loading) return

        const p0 = new PIXI.Point(0, 0)
        const p1 = new PIXI.Point(this.viewport.width, this.viewport.height)
        const topLeft = DoubledCoord.fromCube(this.layout.pixelToHex(p0))
        const bottomRight = DoubledCoord.fromCube(this.layout.pixelToHex(p1))
        const col0 = topLeft.col - 2
        const row0 = Math.max(0, topLeft.row - 2)
        const col1 = bottomRight.col + 2
        const row1 = Math.min(bottomRight.row + 2, this.size.height - 1)

        console.log(col0, row0, col1, row1)

        this.tiles.removeChildren()
        this.outline.removeChildren()
        for (let col = col0; col <= col1 ; col++) {
            for (let row = row0; row <= row1; row++) {
                if ((col + row) % 2) continue

                this.drawTile(col, row)
            }
        }

        this.renderer.render(this.root);
    }
}
