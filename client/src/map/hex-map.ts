import { autoDetectRenderer, Container, DisplayObject, Sprite, Texture, Text, Loader, Point, AbstractRenderer } from 'pixi.js'
import { GlowFilter } from '@pixi/filter-glow'
import { Hex, DoubledCoord, Layout, Orientation } from '../geometry'
import { Link } from '../game/link'
import { Region } from "../game/region"
import { Viewport } from './viewport'
import { TerrainInfo } from '../game/terrain-info'
import { Settlement } from '../game/settlement'

export interface GetRegionCallback {
    (x: number, y: number): Region
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

/*

hex corner indecies

  2  1
3      0
  4  5

*/

class Layers {
    readonly backdrop = new Container()
    readonly terrain = new Container()
    readonly path = new Container()
    readonly settlements = new Container()
    readonly text = new Container()

    add(layer: keyof Omit<Layers, 'add'>, o: DisplayObject) {
        this[layer].addChild(o)
    }
}

class Resources {
    constructor(private loader: Loader) {

    }

    private readonly cache: Map<string, Texture> = new Map()

    sprite(name: string): Sprite {
        const s = new Sprite(this.texture(name))
        s.anchor.set(0.5, 0.5)

        return s
    }

    texture(name: string): Texture | null {
        if (this.cache.has(name)) {
            return this.cache.get(name)
        }

        if (name.includes('/')) {
            const [ key, spriteName ] = name.split('/')

            const sprites = this.loader.resources[key];
            const sheet = sprites.spritesheet

            return sheet.textures[spriteName] ?? sheet.textures[`${spriteName}.png`]
        }

        return this.loader.resources[name]?.texture
    }

    add(name: string, texture: Texture) {
        this.cache.set(name, texture)
    }

    load() {
        this.loader.add('terrain', '/terrain-advisor.json')
    }
}

class Tile {
    public constructor(private region: Region, private layers: Layers, private res: Resources) {
        this.bakcdrop = region.covered
            ? this.res.sprite('terrain/terra-incognita')
            : this.res.sprite('terrain/terra')
        this.layers.add('backdrop', this.bakcdrop)

        if (!region.covered) {
            this.terrain = res.sprite(`terrain/${region.terrain.code}`)
            this.layers.add('terrain', this.terrain)

            if (region.settlement) {
                this.updateSettlement(region.settlement)
            }
        }
    }

    private bakcdrop: Sprite
    private terrain: Sprite
    private settlementLabel: Text
    private setllementMark: Sprite

    active: boolean = false

    private updateSettlement(settlement: Settlement) {
        this.settlementLabel = new Text(settlement.name)
        this.settlementLabel.anchor.set(0, 0.5);

        this.setllementMark = this.res.sprite(`settlement-${settlement.size}`)

        this.layers.add('text', this.settlementLabel)
        this.layers.add('settlements', this.setllementMark)
    }

    update() {
        if (this.active) {
            this.terrain.filters.push(new GlowFilter({
                distance: 2
            }))
        }
        else {
            this.terrain.filters.splice(0)
        }
    }
}

export class HexMap2 {
    constructor(private canvas: HTMLCanvasElement) {

    }

    readonly renderer: AbstractRenderer
}

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
            },
            this.onClick
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
        this.units = new Container()
        this.settlements = new Container()

        this.root.addChild(this.outline)
        this.root.addChild(this.tiles)
        this.root.addChild(this.selected)
        this.root.addChild(this.units)
        this.root.addChild(this.settlements)
    }

    destroy() {
        this.viewport.destroy()
        this.renderer.destroy()
    }

    readonly loader = new Loader()

    readonly root: Container
    readonly tiles: Container
    readonly outline: Container
    readonly selected: Container
    readonly units: Container
    readonly settlements: Container

    readonly renderer: AbstractRenderer
    readonly viewport: Viewport
    readonly layout: Layout

    turnNumber: number

    onRegionSelected: (col: number, row: number) => void

    onClick = (e: MouseEvent) => {
        const x = e.clientX - this.canvas.offsetLeft
        const y = e.clientY - this.canvas.offsetTop

        this.selectedRegion = this.layout.pixelToHex(new Point(x, y))

        this.update()

        const dc = DoubledCoord.fromCube(this.selectedRegion)

        dc.x = dc.x % this.size.width
        if (dc.x < 0) dc.x += this.size.width

        if (this.onRegionSelected) this.onRegionSelected(dc.x, dc.y)
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
        g.lineStyle(2, 0xED2939)
        g.drawPolygon(points)
        this.selected.addChild(g)
    }

    textureVillage: PIXI.Texture
    textureTown: PIXI.Texture
    textureCity: PIXI.Texture

    load() {
        const g1 = new PIXI.Graphics()
        g1.beginFill(0xffffff)
        g1.drawCircle(0, 0, 3)
        g1.endFill()
        this.textureVillage = this.renderer.generateTexture(g1)

        const g2 = new PIXI.Graphics()
        g2.lineStyle(1, 0xffffff)
        g2.drawCircle(0, 0, 5)
        this.textureTown = this.renderer.generateTexture(g2)

        const g3 = new PIXI.Graphics()
        g3.beginFill(0xffffff)
        g3.drawCircle(0, 0, 3)
        g3.endFill()
        g3.lineStyle(1, 0xffffff)
        g3.drawCircle(0, 0, 5)
        this.textureCity = this.renderer.generateTexture(g3)

        return new Promise((resolve, reject) => {
            if (this.loader.resources['/terrain-advisor.json']) {
                return
            }

            this.loader.add('/terrain-advisor.json');
            this.loader.add('/flag.svg');
            this.loader.add('/galleon.svg');

            this.loader.onError.once(reject)

            this.loader.load((_, res) => resolve(res))
        })
    }

    setPath(path: Link[]) {

    }

    terrain(t: TerrainInfo | string): PIXI.Texture {
        const sprites = this.loader.resources['/terrain-advisor.json'];
        const sheet = sprites.spritesheet

        const name = typeof t === 'string' ? t : t.code
        return sheet.textures[`${name}.png`]
    }

    drawSettlement(settlement: Settlement, p: Point) {
        let texture: PIXI.Texture
        switch (settlement.size) {
            case 'village': {
                texture = this.textureVillage
                break
            }

            case 'town': {
                texture = this.textureTown
                break
            }

            case 'city': {
                texture = this.textureCity
                break
            }
        }

        const pin = new PIXI.Sprite(texture)
        pin.position.copyFrom(p)
        this.settlements.addChild(pin)

        const txt = new PIXI.Text(settlement.name, {
            fontSize: '16px',
            fontFamily: 'Almendra',
            fill: 'white',
            dropShadow: true,
            dropShadowColor: '#000000',
            dropShadowAngle: Math.PI / 3,
            dropShadowDistance: 4,
        })
        txt.calculateBounds()

        txt.position.copyFrom(p)
        txt.position.x += pin.width / 2 + 8
        txt.position.y -= (txt.height / 2)

        this.settlements.addChild(txt)
    }

    drawTile(col, row) {
        const dc = new DoubledCoord(col, row)
        const hex = dc.toCube()

        col = col % this.size.width
        if (col < 0) col += this.size.width

        const region = this.getRegion(col, row)
        if (!region || region.covered) return

        const p = this.layout.hexToPixel(hex)
        p.x += OFFSET_X
        p.y += OFFSET_Y

        const tile = new PIXI.Sprite(this.terrain(region.terrain))
        tile.anchor.set(0.5)
        tile.position.copyFrom(p)

        if (!region.isVisible) {
            tile.tint = region.explored
                ? 0xb0b0b0 // darken region when no region report
                : 0x707070 // darken region more when not explored

        }
        this.tiles.addChild(tile)

        const corners = this.layout.polygonCorners(hex)

        const g = new PIXI.Graphics()
        g.lineStyle(3, 0x6f6f6f)
        g.drawPolygon(corners)
        this.outline.addChild(g)

        // settlement
        if (region.settlement) {
            this.drawSettlement(region.settlement, p)
        }

        // units
        if (Array.from(region.troops.values()).some(x => x.faction.isPlayer)) {
            const flag = new PIXI.Sprite(this.loader.resources['/flag.svg'].texture)
            flag.scale.set(0.125)
            flag.anchor.set(0, flag.height * 0.125 / 2)

            const yy = (corners[0].y - corners[1].y) / 4 + corners[1].y

            flag.position.set(p.x, yy)

            this.units.addChild(flag)
        }

        // objects
        if (region.structures.length) {
            const pin = new PIXI.Graphics()

            pin.beginFill(0xff33aa)
            pin.drawCircle(0, 0, 2)
            pin.endFill()

            const offs = (corners[5].y - corners[0].y) / 4
            const yy = corners[5].y - offs
            const xx = corners[4].x + offs

            pin.position.set(xx, yy)

            this.units.addChild(pin)
        }
    }

    update() {
        if (this.loader.loading) return

        const scale = this.viewport.scale
        this.root.scale.set(scale)

        const p0 = new PIXI.Point(0, 0)
        const p1 = new PIXI.Point(this.viewport.width, this.viewport.height)
        const topLeft = DoubledCoord.fromCube(this.layout.pixelToHex(p0))
        const bottomRight = DoubledCoord.fromCube(this.layout.pixelToHex(p1))
        const col0 = topLeft.x - 2
        const row0 = Math.max(0, topLeft.y - 2)
        const col1 = bottomRight.x + 2
        const row1 = Math.min(bottomRight.y + 2, this.size.height - 1)

        this.tiles.removeChildren()
        this.outline.removeChildren()
        this.settlements.removeChildren()
        this.units.removeChildren()

        for (let row = row0; row <= row1; row++) {
            for (let col = col0; col <= col1 ; col++) {
                if ((col + row) % 2) continue

                this.drawTile(col, row)
            }
        }

        this.drawSelectedHex()

        this.renderer.render(this.root);
    }

    centerAt(col: number, row: number) {
        const p = this.layout.hexToPixel(new DoubledCoord(col, row));
        const w = this.viewport.width
        const h = this.viewport.height

        this.viewport.setOffset(new PIXI.Point(
            w / 2 - p.x,
            h / 2 - p.y
        ))
    }
}
