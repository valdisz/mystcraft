import { autoDetectRenderer, Container, DisplayObject, Sprite, Texture, Text, Loader, Point, IPointData, AbstractRenderer, Graphics } from 'pixi.js'
import { GlowFilter } from '@pixi/filter-glow'
import { Hex, Coord, Layout, Orientation } from '../geometry'
import { Link } from '../game/link'
import { Region } from "../game/region"
import { Viewport } from './viewport'
import { TerrainInfo } from '../game/terrain-info'
import { Settlement } from '../game/settlement'
import { Coords } from '../game/coords'
import { Structure } from '../game/structure'

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
    readonly terrain = new Container()
    readonly roads = new Container()
    readonly path = new Container()
    readonly settlements = new Container()
    readonly text = new Container()

    add(layer: LayerName, o: DisplayObject) {
        this[layer].addChild(o)
    }

    clearAll() {
        this.terrain.children.splice(0)
        this.roads.children.splice(0)
        this.path.children.splice(0)
        this.settlements.children.splice(0)
        this.text.children.splice(0)
    }

    sort() {
        this.terrain.sortChildren()
        this.roads.sortChildren()
        this.path.sortChildren()
        this.settlements.sortChildren()
        this.text.sortChildren()
    }
}

type LayerName = keyof Omit<Omit<Omit<Layers, 'add'>, 'clearAll'>, 'sort'>

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

    private loadSpritesheet(name: string, url: string) {
        return new Promise((resolve, reject) => {
            if (this.loader.resources[name]) {
                console.log(`Resource ${name} (${url}) already exists`)
                resolve(this.loader.resources[name])
                return
            }

            this.loader.add(name, url, res => {
                if (res) {
                    resolve(res)
                    console.log(`Loaded ${name} from ${url}`, res)
                }
                else {
                    reject()
                }
            })

            this.loader.load()
        })
    }

    async load() {
        console.log('Loading resources')

        if (!document.fonts.check('12px Fira Code')) {
            document.fonts.load('12px Fira Code')
        }

        if (!document.fonts.check('12px Almendra')) {
            document.fonts.load('12px Almendra')
        }

        await this.loadSpritesheet('terrain', '/terrain.json')
        await this.loadSpritesheet('items', '/items.json'),
        await this.loadSpritesheet('objects', '/objects.json')
        await this.loadSpritesheet('map', '/map.json')

        await document.fonts.ready
    }
}

function arrayEquals(a: any[], b: any[]) {
    return a.length === b.length && a.every((v, i) => v === b[i])
}

abstract class Feature<T = any> {
    constructor(protected readonly layer: LayerName) {

    }

    private _key: any[] = []
    private _graphics: DisplayObject = null

    get key() { return this._key }
    get graphics() { return this._graphics }

    update(value: T, layers: Layers, res: Resources) {
        const key = this.getKey(value)
        if (arrayEquals(this._key, key)) {
            return
        }

        this._key = key
        if (this._graphics) {
            layers[this.layer].removeChild(this._graphics)
            this._graphics.destroy()
        }

        this._graphics = this.getGraphics(value, res)
        if (this._graphics) {
            layers.add(this.layer, this._graphics)
        }
    }

    destroy() {
        if (this._graphics) {
            this._graphics.destroy()
        }
    }

    protected abstract getKey(value: T): any[]
    protected abstract getGraphics(value: T, res: Resources): DisplayObject | null
}

class TerrainFeature extends Feature<Region> {
    constructor(layer: LayerName) {
        super(layer)
    }

    protected getKey(reg: Region): any[] {
        return [ reg.covered, reg.terrain?.code ]
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        let sprite: Sprite
        if (reg.covered) {
            sprite = res.sprite(`terrain/unexplored`)
            sprite.zIndex = -1
        }
        else {
            sprite = res.sprite(`terrain/${reg.terrain.code}`)
        }

        return sprite
    }
}

class SettlementLabelFeature extends Feature<Region> {
    constructor(layer: LayerName) {
        super(layer)
    }

    protected getKey(reg: Region): any[] {
        return [ reg.settlement?.name ]
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        if (!reg.settlement?.name) {
            return
        }

        const text = new Text(reg.settlement.name, {
            fontSize: '16px',
            fontFamily: 'Almendra',
            // fontFamily: 'Fira Code',
            fill: 'white',
            dropShadow: true,
            dropShadowColor: '#000000',
            dropShadowAngle: Math.PI / 3,
            dropShadowDistance: 4,
        })
        text.alpha = 0.9
        text.anchor.set(0, 1)

        return text
    }
}

class SettlementMarkFeature extends Feature<Region> {
    constructor(layer: LayerName) {
        super(layer)
    }

    protected getKey(reg: Region): any[] {
        return [ reg.settlement?.size ]
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        if (!reg.settlement?.size) {
            return
        }

        return res.sprite(`map/${reg.settlement.size}`)
    }
}

class RoadsFeature extends Feature<Region> {
    constructor(layer: LayerName) {
        super(layer)
    }

    protected getKey(reg: Region): any[] {
        return reg.structures.filter(x => x.type.startsWith('Road ')).map(x => x.type.toLowerCase().replace(/\s+/, '-'))
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        if (!this.key.length) {
            return
        }

        const roads = new Container()
        for (const r of this.key) {
            roads.addChild(res.sprite(`terrain/${r}`))
        }

        return roads
    }
}

class Tile {
    public constructor(position: IPointData, private region: Region, private layers: Layers, private res: Resources) {
        this.position.copyFrom(position)

        this.terrain = new TerrainFeature('terrain')
        this.settlementLabel = new SettlementLabelFeature('settlements')
        this.setllementMark = new SettlementMarkFeature('settlements')
        this.roads = new RoadsFeature('roads')

        this.update()
    }

    readonly terrain: Feature<Region>
    readonly settlementLabel: Feature<Region>
    readonly setllementMark: Feature<Region>
    readonly roads: Feature<Region>

    active: boolean = false
    readonly position: Point = new Point()

    private setPosition(target: Feature, x: number = 0, y: number = 0) {
        const g = target.graphics
        if (!g) {
            return
        }

        g.position.set(this.position.x + x, this.position.y + y)
    }

    update() {
        this.terrain.update(this.region, this.layers, this.res)
        this.setPosition(this.terrain, 0, 6)

        this.setllementMark.update(this.region, this.layers, this.res)
        this.setPosition(this.setllementMark)

        this.settlementLabel.update(this.region, this.layers, this.res)
        this.setPosition(this.settlementLabel, 8)

        this.roads.update(this.region, this.layers, this.res)
        this.setPosition(this.roads, 0, 6)
    }

    destroy() {
        this.terrain.destroy()
        this.settlementLabel.destroy()
        this.setllementMark.destroy()
        this.roads.destroy()
    }
}

let lastId = 0
function makeRegion(x: number, y: number, conf: (reg: Region) => void) {
    const reg = new Region((lastId++).toString(), new Coords(x, y, 1))
    conf(reg)

    return reg
}

function addTestData(map: HexMap2) {
    const regions: Region[] = [ ]

    regions.push(makeRegion(0, 0, reg => { reg.terrain = new TerrainInfo('plain') }))
    regions.push(makeRegion(1, 1, reg => {
        reg.terrain = new TerrainInfo('plain')

        const roadS = new Structure();
        roadS.id = '1'
        roadS.num = 1
        roadS.type = 'Road S'

        reg.addStructure(roadS)
    }))
    regions.push(makeRegion(2, 0, reg => { reg.terrain = new TerrainInfo('forest') }))
    regions.push(makeRegion(0, 2, reg => { reg.terrain = new TerrainInfo('plain') }))
    regions.push(makeRegion(1, 3, reg => {
        reg.terrain = new TerrainInfo('plain')
        reg.settlement = {
            name: 'Avalon',
            size: 'city'
        }

        const roadN = new Structure();
        roadN.id = '1'
        roadN.num = 1
        roadN.type = 'Road N'

        reg.addStructure(roadN)
    }))
    regions.push(makeRegion(2, 2, reg => { reg.terrain = new TerrainInfo('forest') }))
    regions.push(makeRegion(0, 4, reg => { reg.terrain = new TerrainInfo('volcano') }))
    regions.push(makeRegion(2, 4, reg => { reg.terrain = new TerrainInfo('plain') }))
    regions.push(makeRegion(3, 1, reg => { reg.terrain = new TerrainInfo('plain') }))
    regions.push(makeRegion(3, 3, reg => { reg.terrain = new TerrainInfo('plain') }))
    regions.push(makeRegion(3, 5, reg => { reg.terrain = new TerrainInfo('plain') }))
    regions.push(makeRegion(4, 4, reg => { reg.terrain = new TerrainInfo('plain') }))

    for (let x = 0; x < 32; x++) {
        for (let y = 0; y < 32; y++) {
            if ((x + y) % 2) {
                continue
            }

            if (regions.find(r => r.coords.x === x && r.coords.y === y)) {
                continue
            }

            regions.push(makeRegion(x, y, reg => reg.covered = true))
        }
    }

    map.setRegions(regions)
}

export class HexMap2 {
    constructor(private canvas: HTMLCanvasElement) {
        this.renderer = autoDetectRenderer({
            width: this.canvas.width,
            height: this.canvas.height,
            view: canvas,
            antialias: true,
            resolution: window.devicePixelRatio || 1
        })

        this.resources = new Resources(new Loader())

        this.scene = new Container()

        this.layers = new Layers()
        this.scene.addChild(this.layers.terrain)
        this.scene.addChild(this.layers.roads)
        this.scene.addChild(this.layers.path)
        this.scene.addChild(this.layers.settlements)
        this.scene.addChild(this.layers.text)

        this.layout = new Layout({ x: 24, y: 24 }, { x: 48, y: 48})
    }

    readonly renderer: AbstractRenderer
    readonly resources: Resources
    readonly layout: Layout

    readonly scene: Container
    readonly layers: Layers

    readonly tiles: Tile[] = []

    load() {
        return this.resources
            .load()
            .then(() => {
                this.clearAll()
                addTestData(this)
            })
    }

    clearAll() {
        this.layers.clearAll()

        for (const tile of this.tiles) {
            tile.destroy()
        }
        this.tiles.splice(0)
    }

    setRegions(regions: Region[]) {
        this.clearAll()

        regions.sort((a, b) => {
            if (a.coords.y == b.coords.y) {
                return a.coords.x - b.coords.x
            }

            return a.coords.y - b.coords.y
        })

        for (const reg of regions) {
            const p = this.layout.toPixel(reg.coords)
            this.tiles.push(new Tile(p, reg, this.layers, this.resources))
        }

        this.layers.sort()
    }

    render() {
        requestAnimationFrame(() => {
            this.scene.position.set(0, 0)
            this.renderer.render(this.scene)

            this.scene.position.set(this.scene.width - 13, 0)
            this.renderer.render(this.scene, { clear: false })
        })
    }

    resize(width: number, height: number) {
        this.renderer.resize(width, height)
    }

    destroy() {
        this.clearAll()

        this.renderer.destroy()
    }
}

// export class HexMap {
//     constructor(private canvas: HTMLCanvasElement, public readonly size: MapSize, public readonly getRegion: GetRegionCallback) {
//         const origin = new Point(TILE_W / 2, TILE_H)

//         this.layout = new Layout(
//             Orientation.FLAT,
//             new Point(TILE_W / 2, TILE_H / 2),
//             origin
//         )

//         const sz = this.layout.hexToPixel(new Coord(size.width - 1, size.height - 1))
//         const maxWidth = sz.x + this.layout.size.x
//         const maxHeight = sz.y + this.layout.size.y

//         this.viewport = new Viewport(canvas,
//             origin,
//             maxWidth,
//             maxHeight,
//             vp => {
//                 this.layout.origin.x = vp.origin.x
//                 this.layout.origin.y = vp.origin.y

//                 this.renderer.resize(vp.width, vp.height)

//                 this.update()
//             },
//             this.onClick
//         )

//         this.renderer = autoDetectRenderer({
//             width: this.viewport.width,
//             height: this.viewport.height,
//             view: canvas,
//             antialias: true,
//             resolution: window.devicePixelRatio || 1
//         })

//         this.root = new Container()
//         this.tiles = new Container()
//         this.outline = new Container()
//         this.selected = new Container()
//         this.units = new Container()
//         this.settlements = new Container()

//         this.root.addChild(this.outline)
//         this.root.addChild(this.tiles)
//         this.root.addChild(this.selected)
//         this.root.addChild(this.units)
//         this.root.addChild(this.settlements)
//     }

//     destroy() {
//         this.viewport.destroy()
//         this.renderer.destroy()
//     }

//     readonly loader = new Loader()

//     readonly root: Container
//     readonly tiles: Container
//     readonly outline: Container
//     readonly selected: Container
//     readonly units: Container
//     readonly settlements: Container

//     readonly renderer: AbstractRenderer
//     readonly viewport: Viewport
//     readonly layout: Layout

//     turnNumber: number

//     onRegionSelected: (col: number, row: number) => void

//     onClick = (e: MouseEvent) => {
//         const x = e.clientX - this.canvas.offsetLeft
//         const y = e.clientY - this.canvas.offsetTop

//         this.selectedRegion = this.layout.pixelToHex(new Point(x, y))

//         this.update()

//         const dc = Coord.fromCube(this.selectedRegion)

//         dc.x = dc.x % this.size.width
//         if (dc.x < 0) dc.x += this.size.width

//         if (this.onRegionSelected) this.onRegionSelected(dc.x, dc.y)
//     }

//     selectedRegion: Hex

//     // selectRegion(col: number, row: number) {
//     //     this.selectedRegion = new DoubledCoord(col, row).toCube()
//     //     this.update()

//     //     if (this.onRegionSelected) this.onRegionSelected()
//     // }

//     drawSelectedHex() {
//         this.selected.removeChildren()

//         if (!this.selectedRegion) return

//         const points = this.layout.polygonCorners(this.selectedRegion)

//         const g = new PIXI.Graphics()
//         g.lineStyle(2, 0xED2939)
//         g.drawPolygon(points)
//         this.selected.addChild(g)
//     }

//     textureVillage: PIXI.Texture
//     textureTown: PIXI.Texture
//     textureCity: PIXI.Texture

//     load() {
//         const g1 = new PIXI.Graphics()
//         g1.beginFill(0xffffff)
//         g1.drawCircle(0, 0, 3)
//         g1.endFill()
//         this.textureVillage = this.renderer.generateTexture(g1)

//         const g2 = new PIXI.Graphics()
//         g2.lineStyle(1, 0xffffff)
//         g2.drawCircle(0, 0, 5)
//         this.textureTown = this.renderer.generateTexture(g2)

//         const g3 = new PIXI.Graphics()
//         g3.beginFill(0xffffff)
//         g3.drawCircle(0, 0, 3)
//         g3.endFill()
//         g3.lineStyle(1, 0xffffff)
//         g3.drawCircle(0, 0, 5)
//         this.textureCity = this.renderer.generateTexture(g3)

//         return new Promise((resolve, reject) => {
//             if (this.loader.resources['/terrain-advisor.json']) {
//                 return
//             }

//             this.loader.add('/terrain-advisor.json');
//             this.loader.add('/flag.svg');
//             this.loader.add('/galleon.svg');

//             this.loader.onError.once(reject)

//             this.loader.load((_, res) => resolve(res))
//         })
//     }

//     setPath(path: Link[]) {

//     }

//     terrain(t: TerrainInfo | string): PIXI.Texture {
//         const sprites = this.loader.resources['/terrain-advisor.json'];
//         const sheet = sprites.spritesheet

//         const name = typeof t === 'string' ? t : t.code
//         return sheet.textures[`${name}.png`]
//     }

//     drawSettlement(settlement: Settlement, p: Point) {
//         let texture: PIXI.Texture
//         switch (settlement.size) {
//             case 'village': {
//                 texture = this.textureVillage
//                 break
//             }

//             case 'town': {
//                 texture = this.textureTown
//                 break
//             }

//             case 'city': {
//                 texture = this.textureCity
//                 break
//             }
//         }

//         const pin = new PIXI.Sprite(texture)
//         pin.position.copyFrom(p)
//         this.settlements.addChild(pin)

//         const txt = new PIXI.Text(settlement.name, {
//             fontSize: '16px',
//             fontFamily: 'Almendra',
//             fill: 'white',
//             dropShadow: true,
//             dropShadowColor: '#000000',
//             dropShadowAngle: Math.PI / 3,
//             dropShadowDistance: 4,
//         })
//         txt.calculateBounds()

//         txt.position.copyFrom(p)
//         txt.position.x += pin.width / 2 + 8
//         txt.position.y -= (txt.height / 2)

//         this.settlements.addChild(txt)
//     }

//     drawTile(col, row) {
//         const dc = new Coord(col, row)
//         const hex = dc.toCube()

//         col = col % this.size.width
//         if (col < 0) col += this.size.width

//         const region = this.getRegion(col, row)
//         if (!region || region.covered) return

//         const p = this.layout.hexToPixel(hex)
//         p.x += OFFSET_X
//         p.y += OFFSET_Y

//         const tile = new PIXI.Sprite(this.terrain(region.terrain))
//         tile.anchor.set(0.5)
//         tile.position.copyFrom(p)

//         if (!region.isVisible) {
//             tile.tint = region.explored
//                 ? 0xb0b0b0 // darken region when no region report
//                 : 0x707070 // darken region more when not explored

//         }
//         this.tiles.addChild(tile)

//         const corners = this.layout.polygonCorners(hex)

//         const g = new PIXI.Graphics()
//         g.lineStyle(3, 0x6f6f6f)
//         g.drawPolygon(corners)
//         this.outline.addChild(g)

//         // settlement
//         if (region.settlement) {
//             this.drawSettlement(region.settlement, p)
//         }

//         // units
//         if (Array.from(region.troops.values()).some(x => x.faction.isPlayer)) {
//             const flag = new PIXI.Sprite(this.loader.resources['/flag.svg'].texture)
//             flag.scale.set(0.125)
//             flag.anchor.set(0, flag.height * 0.125 / 2)

//             const yy = (corners[0].y - corners[1].y) / 4 + corners[1].y

//             flag.position.set(p.x, yy)

//             this.units.addChild(flag)
//         }

//         // objects
//         if (region.structures.length) {
//             const pin = new PIXI.Graphics()

//             pin.beginFill(0xff33aa)
//             pin.drawCircle(0, 0, 2)
//             pin.endFill()

//             const offs = (corners[5].y - corners[0].y) / 4
//             const yy = corners[5].y - offs
//             const xx = corners[4].x + offs

//             pin.position.set(xx, yy)

//             this.units.addChild(pin)
//         }
//     }

//     update() {
//         if (this.loader.loading) return

//         const scale = this.viewport.scale
//         this.root.scale.set(scale)

//         const p0 = new PIXI.Point(0, 0)
//         const p1 = new PIXI.Point(this.viewport.width, this.viewport.height)
//         const topLeft = Coord.fromCube(this.layout.pixelToHex(p0))
//         const bottomRight = Coord.fromCube(this.layout.pixelToHex(p1))
//         const col0 = topLeft.x - 2
//         const row0 = Math.max(0, topLeft.y - 2)
//         const col1 = bottomRight.x + 2
//         const row1 = Math.min(bottomRight.y + 2, this.size.height - 1)

//         this.tiles.removeChildren()
//         this.outline.removeChildren()
//         this.settlements.removeChildren()
//         this.units.removeChildren()

//         for (let row = row0; row <= row1; row++) {
//             for (let col = col0; col <= col1 ; col++) {
//                 if ((col + row) % 2) continue

//                 this.drawTile(col, row)
//             }
//         }

//         this.drawSelectedHex()

//         this.renderer.render(this.root);
//     }

//     centerAt(col: number, row: number) {
//         const p = this.layout.hexToPixel(new Coord(col, row));
//         const w = this.viewport.width
//         const h = this.viewport.height

//         this.viewport.setOffset(new PIXI.Point(
//             w / 2 - p.x,
//             h / 2 - p.y
//         ))
//     }
// }
