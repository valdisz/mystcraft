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

// // fantasy
// const TILE_W = 32;
// const TILE_H = 32;
// const OFFSET_X = 0;
// const OFFSET_Y = -8;

// advisor
const TILE_W = 48;
const TILE_H = 48;
const OFFSET_X = 0;
const OFFSET_Y = 0;

export class HexMap {
    constructor(private canvas: HTMLCanvasElement, public readonly size: MapSize, public readonly getRegion: GetRegionCallback) {
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
        this.selected = new Container()
        this.root.addChild(this.outline)
        this.root.addChild(this.tiles)
        this.root.addChild(this.selected)

        canvas.addEventListener('click', this.onClick)
    }

    readonly loader = new Loader()

    readonly root: Container
    readonly tiles: Container
    readonly outline: Container
    readonly selected: Container

    readonly renderer: Renderer
    readonly viewport: Viewport
    readonly layout: Layout

    onRegionSelected: (col: number, row: number) => void

    onClick = (e: MouseEvent) => {
        const x = e.clientX - this.canvas.offsetLeft
        const y = e.clientY - this.canvas.offsetTop

        this.selectedRegion = this.layout.pixelToHex(new PIXI.Point(x, y))

        this.update()

        const dc = DoubledCoord.fromCube(this.selectedRegion)

        dc.col = dc.col % this.size.width
        if (dc.col < 0) dc.col += this.size.width

        if (this.onRegionSelected) this.onRegionSelected(dc.col, dc.row)
    }

    selectedRegion: Hex

    // selectRegion(col: number, row: number) {
    //     this.selectedRegion = new DoubledCoord(col, row).toCube()
    //     this.update()

    //     if (this.onRegionSelected) this.onRegionSelected()
    // }

    drawSelectedHex() {
        this.selected.removeChildren()

        if (!this.selectedRegion) return

        const points = this.layout.polygonCorners(this.selectedRegion)

        const g = new PIXI.Graphics()
        g.lineStyle(6, 0x1E4EFF, 0.75)
        g.drawPolygon(points)
        this.selected.addChild(g)

        const g2 = new PIXI.Graphics()
        // g2.lineStyle(2, 0xD46A6A, 0.75)
        g2.lineStyle(2, 0xAEBFFF, 0.5)
        g2.drawPolygon(points)
        this.selected.addChild(g2)
    }

    load() {
        return new Promise((resolve, reject) => {
            this.loader.add('/terrain-advisor.json');

            this.loader.onError.once(reject)

            this.loader.load((_, res) => resolve(res))
        })
    }

    getTerrainTexture(name: string): PIXI.Texture {
        const sprites = this.loader.resources['/terrain-advisor.json'];
        const sheet = sprites.spritesheet
        return sheet.textures[`${name}.png`]
    }

    drawTile(col, row) {
        const dc = new DoubledCoord(col, row)
        const hex = dc.toCube()

        col = col % this.size.width
        if (col < 0) col += this.size.width

        const region = this.getRegion(col, row)
        if (region) {
            const p = this.layout.hexToPixel(hex)
            p.x += OFFSET_X
            p.y += OFFSET_Y

            const tile = new PIXI.Sprite(this.getTerrainTexture(region.terrain))
            tile.anchor.set(0.5)
            tile.position = p
            this.tiles.addChild(tile)

            const g = new PIXI.Graphics()
            g.lineStyle(4, 0x999999)
            g.drawPolygon(this.layout.polygonCorners(hex))
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

        this.tiles.removeChildren()
        this.outline.removeChildren()
        for (let row = row0; row <= row1; row++) {
        for (let col = col0; col <= col1 ; col++) {
                if ((col + row) % 2) continue

                this.drawTile(col, row)
            }
        }

        this.drawSelectedHex()

        this.renderer.render(this.root);
    }
}
