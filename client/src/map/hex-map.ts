import { autoDetectRenderer, Container, Loader, AbstractRenderer, Point, Rectangle, IPointData } from 'pixi.js'
import { Region } from "../game"
import { Layers } from './layers'
import { Resources } from './resources'
import { Tile, TILE_H, TILE_W } from './tile'
import { Viewport } from './viewport'
import { overlapping } from './utils'
import { Layout } from '../geometry'
import { ICoords } from '../game'
import { MapState } from './state'

export interface GetRegionCallback {
    (x: number, y: number): Region
}

export interface MapSize {
    width: number
    height: number
}

/*

hex corner indecies

  2  1
3      0
  4  5

*/

export interface HexMapOptions {
    onClick?: (reg: Region) => void
    onDblClick?: (reg: Region) => void
}

export class HexMap2 implements MapState {
    constructor(private canvas: HTMLCanvasElement, private readonly resources: Resources, private readonly mapWidth: number, private readonly mapHeight: number,
        private readonly options?: HexMapOptions
    ) {
        const origin = { x: 50, y: 50 }

        this.layout = new Layout(null, origin)
        this.updateLayout(this.zoom, this.zoom)

        const wh = this.toPixel({ x: mapWidth - 1, y: mapHeight - 1, z: 0 })

        // todo: get rid of magic number in the map width
        this.viewport = new Viewport(this.canvas, origin, wh.x + 22, wh.y,
            vp => {
                this.renderer.resize(vp.width, vp.height)
                this.render()
            },
            (e, vp) => {
                e.preventDefault()
                e.stopPropagation()
                const tile = this.getTileAtPixel(e.offsetX, e.offsetY)
                if (tile) {
                    if (options?.onClick) {
                        options.onClick(tile.reg)
                    }
                }
            }
        )
        this.updateViewport(this.zoom, this.zoom)

        this.canvas.addEventListener('dblclick', this.onDblClick)

        this.renderer = autoDetectRenderer({
            width: this.viewport.width,
            height: this.viewport.height,
            view: canvas,
            antialias: true,
            resolution: window.devicePixelRatio || 1
        })

        this.scene = new Container()

        this.layers = new Layers()
        this.scene.addChild(this.layers.terrain)
        this.scene.addChild(this.layers.roads)
        this.scene.addChild(this.layers.path)
        this.scene.addChild(this.layers.features)
        this.scene.addChild(this.layers.highlight)
        this.scene.addChild(this.layers.settlements)
        this.scene.addChild(this.layers.text)
    }

    readonly viewport: Viewport
    readonly layout: Layout
    readonly renderer: AbstractRenderer

    readonly scene: Container
    readonly layers: Layers

    private readonly index: number[] = []
    readonly tiles: Tile[] = []

    selectedTile: Tile
    zoom: number = 1

    zoomIn() {
        this.setZoom(this.zoom / 2)
    }

    zoomOut() {
        this.setZoom(this.zoom * 2)
    }

    private updateViewport(oldZoom: number, newZoom: number) {
        const scale = oldZoom / newZoom

        this.viewport.mapWidth = this.viewport.mapWidth * scale
        this.viewport.mapHeight = this.viewport.mapHeight * scale

        const { x, y } = this.viewport.origin
        this.viewport.origin.set(x * scale, y * scale)
        this.layout.origin.copyFrom(this.viewport.origin)
    }

    private updateLayout(oldZoom: number, newZoom: number) {
        const scale = oldZoom / newZoom

        const size = 48 / this.zoom
        this.layout.size.set(size, size)
    }

    private setZoom(zoom: number) {
        const oldZoom = this.zoom
        const newZoom = Math.max(1, Math.min(zoom, 4))
        if (this.zoom != newZoom) {
            this.zoom = newZoom

            this.updateViewport(oldZoom, newZoom)
            this.updateLayout(oldZoom, newZoom)
            this.udateTiles()
            this.render()
        }
    }

    select(coords?: ICoords) {
        if (this.selectedTile) {
            this.selectedTile.isActive = false
            this.selectedTile.update()
        }

        if (coords) {
            const tile = this.getTile(coords.x, coords.y)
            if (tile) {
                this.selectedTile = tile
                this.selectedTile.isActive = true
                this.selectedTile.update()
            }
        }

        this.render()
    }

    centerAt(coords: ICoords) {
        if (!coords) {
            return
        }

        const p = this.toPixel(coords)
        this.viewport.updateBounds(-(p.x - this.viewport.width / 2), -(p.y - this.viewport.height / 2))
    }

    private onDblClick = (e: MouseEvent) => {
        e.preventDefault()
        e.stopPropagation()

        const tile = this.getTileAtPixel(e.offsetX, e.offsetY)
        if (tile) {
            if (this.options?.onDblClick) {
                this.options.onDblClick(tile.reg)
            }
        }
    }

    private getTileIndex(x: number, y: number) {
        return x + y * this.mapWidth / 2
    }

    private getTile(x: number, y: number) {
        if (y < 0) {
            return null
        }

        if (y >= this.mapHeight) {
            return null
        }

        let xx = x % this.mapWidth
        if (xx < 0) {
            xx += this.mapWidth
        }

        const ti = this.getTileIndex(xx, y)
        const i = this.index[ti]
        return this.tiles[i]
    }

    private getTileAtPixel(x: number, y: number) {
        const coord = this.fromPixel(x, y)
        const tile = this.getTile(coord.x, coord.y)
        return tile
    }

    toPixel(coords: ICoords): IPointData {
        return this.layout.toPixel(coords)
    }

    fromPixel(x: number, y: number) {
        const origin = this.viewport.origin
        return this.layout.toCoord({ x: x - origin.x, y: y - origin.y })
    }

    clearAll() {
        this.selectedTile = null
        this.layers.clearAll()

        for (const tile of this.tiles) {
            tile.destroy()
        }

        this.tiles.splice(0)
        this.index.splice(0)
    }

    setRegions(regions: Region[]) {
        this.clearAll()

        regions.sort((a, b) => {
            if (a.coords.y == b.coords.y) {
                return a.coords.x - b.coords.x
            }

            return a.coords.y - b.coords.y
        })

        this.index.length = this.mapWidth * this.mapHeight

        for (const reg of regions) {
            const p = this.toPixel(reg.coords)
            const t = new Tile(p, reg, this.layers, this.resources, this)

            this.index[this.getTileIndex(reg.coords.x, reg.coords.y)] = this.tiles.length
            this.tiles.push(t)

            t.update()
        }

        this.layers.sort()
    }

    // todo: fix issue with wraparound map edges
    render() {
        requestAnimationFrame(() => {
            const { x, y } = this.viewport.origin

            this.scene.position.set(x, y)
            this.updateVisibility()
            this.renderer.render(this.scene)

            this.scene.position.set(x - this.viewport.mapWidth, y)
            this.updateVisibility()
            this.renderer.render(this.scene, { clear: false })

            this.scene.position.set(x + this.viewport.mapWidth, y)
            this.updateVisibility()
            this.renderer.render(this.scene, { clear: false })
        })
    }

    updateVisibility() {
        const p = new Point()
        const scale = 1 / this.zoom

        const viewRect = this.viewport.rect
        const r = new Rectangle()

        for (const layer of this.scene.children as Container[]) {
            for (const o of layer.children as Container[]) {
                o.toGlobal(this.viewport.origin, p)

                r.x = p.x - TILE_W * scale
                r.y = p.y - TILE_H * scale
                r.width = o.width + TILE_W * scale
                r.height = o.height + TILE_H * scale

                o.visible = overlapping(viewRect, r)
            }
        }
    }

    resize(width: number, height: number) {
        this.renderer.resize(width, height)
    }

    udateTiles() {
        for (const tile of this.tiles) {
            tile.position.copyFrom(this.toPixel(tile.reg.coords))
            tile.update()
        }
    }

    destroy() {
        this.clearAll()

        this.renderer.destroy()
        this.canvas.removeEventListener('dblclick', this.onDblClick)
    }
}
