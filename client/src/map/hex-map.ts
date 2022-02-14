import { autoDetectRenderer, Container, DisplayObject, Sprite, Texture, Text, Loader, Point, IPointData, AbstractRenderer, Graphics, Spritesheet, ISpritesheetData, BLEND_MODES } from 'pixi.js'
import { GlowFilter } from '@pixi/filter-glow'
import { BlurFilter } from '@pixi/filter-blur'
import { Hex, Coord, Layout, Orientation } from '../geometry'
import { Link } from '../game/link'
import { Region } from "../game/region"
import { Viewport } from './viewport'
import { TerrainInfo } from '../game/terrain-info'
import { Settlement } from '../game/settlement'
import { Coords } from '../game/coords'
import { Structure } from '../game/structure'

import rulesetData from './ruleset.yaml'
import report from './report.json'
import { Faction } from '../game/faction'
import { Ruleset } from '../game/ruleset'
import { World } from '../game/world'
import { Unit } from '../game/unit'

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
const TILE_W = 94;
const TILE_H = 82;

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

interface TailSetSpec {
    main: number
}

interface TailSetManifest extends ISpritesheetData {
    tiles: {
        [name: string]: TailSetSpec
    }
}

class TileSet {
    constructor(private readonly spritesheet: Spritesheet) {

    }

    mainTile(name: string, hash: number) {
        if (name === 'ocean') {
            name = 'water'
        }

        const { tiles } = this.spritesheet.data as TailSetManifest
        const frameCount = tiles[name].main
        const index = hash % frameCount

        const frameName = `${name}-main-${index}`
        const texture = this.spritesheet.textures[frameName]

        const sprite = new Sprite(texture)
        sprite.anchor.set(0.5, 0.5)

        return sprite
    }
}

class Resources {
    constructor(private loader: Loader) {

    }

    private readonly cache: Map<string, Texture> = new Map()
    private tileSet: TileSet

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

    tile(name: string, hash: number): Sprite {
        return this.tileSet.mainTile(name, hash)
    }

    add(name: string, texture: Texture) {
        this.cache.set(name, texture)
    }

    private queueSpritesheet(name: string, url: string) {
        return new Promise((resolve, reject) => {
            if (this.loader.resources[name]) {
                console.log(`Resource ${name} (${url}) already exists`)
                resolve(this.loader.resources[name])
                return
            }

            this.loader.add(name, url, res => {
                if (res) {
                    this.tileSet = new TileSet(this.loader.resources['sprites'].spritesheet)

                    resolve(res)
                    console.log(`Loaded ${name} from ${url}`, res)
                }
                else {
                    reject()
                }
            })
        })
    }

    private queueFont(name: string, size: string = '12px') {
        const fontFamily = `${size} ${name}`
        if (!document.fonts.check(fontFamily)) {
            document.fonts.load(fontFamily)
        }
    }

    async load() {
        console.log('Loading resources')

        this.queueFont('Fira Code')
        this.queueFont('Almendra')

        const tasks = Promise.all([
            document.fonts.ready,
            this.queueSpritesheet('sprites', '/sprites.json'),
        ])

        this.loader.load()

        return tasks
    }
}

function arrayEquals(a: any[], b: any[]) {
    return a.length === b.length && a.every((v, i) => v === b[i])
}

abstract class Feature<T = any> {
    constructor(protected readonly layer: LayerName, position: IPointData) {
        this.position.copyFrom(position)
    }

    private _key: any[] = []
    private _graphics: DisplayObject = null

    get key() { return this._key }
    get graphics() { return this._graphics }

    readonly position: Point = new Point()

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
            this._graphics.position.copyFrom(this.position)
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
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)

        this.hash = Math.abs(pointHash(position))
    }

    readonly hash: number

    protected getKey(reg: Region): any[] {
        return [ reg.covered, reg.terrain?.code ]
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        let sprite: Sprite
        if (reg.covered) {
            sprite = res.tile('unexplored', this.hash)
            sprite.zIndex = -1
        }
        else {
            sprite = res.tile(reg.terrain.code, this.hash)
        }

        return sprite
    }
}

class SettlementFeature extends Feature<Region> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    protected getKey(reg: Region): any[] {
        return [ reg.settlement?.name, reg.settlement?.size ]
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        if (!reg.settlement?.name) {
            return
        }

        const p = new Point(12, 0)

        const text = new Text(reg.settlement.name, {
            fontSize: '16px',
            // fontFamily: 'Almendra',
            fontFamily: 'Fira Code',
            fill: 'yellow',
            fontWeight: 'bold',
            dropShadow: true,
            dropShadowColor: 'black',
            dropShadowBlur: 4,
            dropShadowDistance: 2
        })
        text.anchor.set(0, 0.5)
        text.position.copyFrom(p)

        const group = new Container()
        group.addChild(res.sprite(`sprites/map-${reg.settlement.size}`))
        group.addChild(text)

        return group
    }
}

class RoadsFeature extends Feature<Region> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    protected getKey(reg: Region): any[] {
        return reg.structures.filter(x => x.type.startsWith('Road ')).map(x => x.type)
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        if (!this.key.length) {
            return
        }

        const roads: string[] = []
        if (this.key.includes('Road N')) roads.push('n')
        if (this.key.includes('Road NW')) roads.push('nw')
        if (this.key.includes('Road NE')) roads.push('ne')
        if (this.key.includes('Road S')) roads.push('s')
        if (this.key.includes('Road SW')) roads.push('sw')
        if (this.key.includes('Road SE')) roads.push('se')

        if (!roads.length) {
            return null
        }

        const spriteName = `sprites/road-${roads.join('-')}`
        const sprite = res.sprite(spriteName)
        sprite.tint = 0x666666

        return sprite
    }
}

interface StructuresFeatureOptions {
    spriteName: string
    countPosition?: 'top' | 'bottom'
    hideCount?: boolean
    matchStructure: (str: Structure) => boolean
}

class StructuresFeature extends Feature<Region> {
    constructor(layer: LayerName, position: IPointData, private readonly options: StructuresFeatureOptions) {
        super(layer, position)
    }

    protected getKey(reg: Region): any[] {
        if (reg.covered) return [ ]
        return reg.structures.filter(x => this.options.matchStructure(x))
        // return [ 1 ]
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        if (!this.key.length) {
            return
        }

        const group = new Container()
        const icon = res.sprite(this.options.spriteName)
        icon.zIndex = 1
        group.addChild(icon)

        if (!this.options.hideCount) {
            const countStr = this.key.length > 9
                ? '9+'
                : this.key.length.toString()

            const count = new Text(countStr, {
                fontSize: '10px',
                fontFamily: 'Fira Code',
                fill: 'black',
                fontWeight: 'bold'
            })
            count.anchor.set(0.5, 0.5)

            if (this.options.countPosition === 'top') {
                icon.position.set(0, -6)
                count.position.set(0, -16)
            }
            else {
                count.position.set(0, -6)
                icon.position.set(0, -16)
            }

            const bg = res.sprite('sprites/map-bg-1')
            bg.alpha = 0.67
            bg.position.copyFrom(count)

            group.addChild(bg)
            group.addChild(count)
            group.sortChildren()
        }

        return group
    }
}

interface MenCount {
    own: number
    hostile: number
    unfriendly: number
    neutral: number
    friendly: number
    ally: number
}

class TroopsFeature extends Feature<Region> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    static attitudeCount(value: MenCount, unit: Unit): MenCount {
        const next = { ...value }

        const key = unit.isPlayer ? 'own' : unit.faction.stance.toLowerCase()
        next[key] += unit.inventory.menCount

        return next
    }

    protected getKey(reg: Region): any[] {
        const { own, hostile, unfriendly, neutral, friendly, ally } = reg.units
            .reduce(
                (value, next) => TroopsFeature.attitudeCount(value, next),
                { own: 0, hostile: 0, unfriendly: 0, neutral: 0, friendly: 0, ally: 0 } as MenCount
            )

        return [ own, hostile, unfriendly, neutral, friendly, ally ]
    }

    static formatOwn(count: number) {
        if (count < 3000) return count.toString()
        if (count < 10000) return `${(count / 1000).toPrecision(1)}k`

        return `${Math.trunc(count / 1000).toFixed(0)}k`
    }

    static formatOther(count: number) {
        if (count === 0) return '.'
        if (count < 100) return count.toString()
        if (count < 1000) return `${Math.trunc(count / 100).toFixed(0)}h`

        return `${Math.trunc(count / 1000).toFixed(0)}k`
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        const [ own, ...other ] = this.key as number[]
        if (!own && other.every(x => x === 0)) {
            return
        }

        const group = new Container()

        if (own) {
            const ownCountText = new Text(TroopsFeature.formatOwn(own), {
                fontSize: '10px',
                fontFamily: 'Fira Code',
                fill: 'lime',
                fontWeight: 'bold',
                dropShadow: true,
                dropShadowColor: 'black',
                dropShadowBlur: 4,
                dropShadowDistance: 2
            })
            ownCountText.anchor.set(0.5, 0.5)
            ownCountText.position.set(0, -6)

            group.addChild(ownCountText)
        }

        if (other.some(x => x !== 0)) {
            const s = other.map(x => TroopsFeature.formatOther(x)).join(' ')
            const otherCountText = new Text(s, {
                fontSize: '8px',
                fontFamily: 'Fira Code',
                fill: 'yellow',
                fontWeight: 'bold',
                dropShadow: true,
                dropShadowColor: 'black',
                dropShadowBlur: 4,
                dropShadowDistance: 2
            })
            otherCountText.anchor.set(0.5, 0.5)
            otherCountText.position.set(0, 6)

            group.addChild(otherCountText)
        }

        return group
    }
}

class OnGuardFeature extends Feature<Region> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)

        this.hash = Math.abs(pointHash(position))
    }

    readonly hash: number

    protected getKey(reg: Region): any[] {
        return [ reg.units.some(x => x.onGuard) ]
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        if (!this.key[0]) {
            return
        }

        return res.sprite('sprites/map-guard')
    }
}

class GateFeature extends Feature<Region> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)

        this.hash = Math.abs(pointHash(position))
    }

    readonly hash: number

    protected getKey(reg: Region): any[] {
        return [ reg.gate > 0 ]
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        if (!this.key[0]) {
            return
        }

        return res.sprite('sprites/map-gate')
    }
}

function numhash(value: number) {
    value = ((value >> 16) ^ value) * 0x45d9f3b
    value = ((value >> 16) ^ value) * 0x45d9f3b
    value = (value >> 16) ^ value

    return value
}

function pointHash({ x, y}: IPointData) {
    let seed = 1430287
    seed = seed * 7302013 ^ numhash(x)
    seed = seed * 7302013 ^ numhash(y)

    return seed
}


const FORTS = [ 'Tower', 'Fort', 'Castle', 'Citadel' ]
const TRADE = [ 'Inn', 'Farm', 'Ranch', 'Quarry', 'Mine' ]
const SHIP = [ 'Longboat' ]
const FLYINGSHIP = [ 'Baloon' ]
const LAIRS = [ 'Lair' ]

class Tile {
    public constructor(position: IPointData, private region: Region, private layers: Layers, private res: Resources) {
        this.position.copyFrom(position)

        this.terrain = new TerrainFeature('terrain', this.getPos(0, -12))   // need to offest terrain tile
        this.roads = new RoadsFeature('roads', this.getPos(0, -12))
        this.settlement = new SettlementFeature('settlements', this.getPos(0, 0))

        this.baloon = new StructuresFeature('text', this.getPos(-18, TILE_H / 2), {
            spriteName: 'sprites/map-flying-ship',
            countPosition: 'top',
            matchStructure: str => FLYINGSHIP.includes(str.type)
        })

        this.ship = new StructuresFeature('text', this.getPos(-6, TILE_H / 2), {
            spriteName: 'sprites/map-ship',
            countPosition: 'top',
            matchStructure: str => SHIP.includes(str.type)
        })

        this.trade = new StructuresFeature('text', this.getPos(6, TILE_H / 2), {
            spriteName: 'sprites/map-building',
            countPosition: 'top',
            matchStructure: str => TRADE.includes(str.type)
        })

        this.forifications = new StructuresFeature( 'text', this.getPos(18, TILE_H / 2), {
            spriteName: 'sprites/map-fort',
            countPosition: 'top',
            matchStructure: str => FORTS.includes(str.type)
        })

        this.lair = new StructuresFeature('text', this.getPos(0, 0), {
            spriteName: 'sprites/map-lair',
            hideCount: true,
            matchStructure: str => LAIRS.includes(str.type)
        })

        this.troops = new TroopsFeature('text', this.getPos(0, 20 - TILE_H / 2))
        this.onGuard = new OnGuardFeature('text', this.getPos(-TILE_W / 2 + 12, 0))
        this.gate = new GateFeature('text', this.getPos(-TILE_W / 2 + 24, 0))

        this.update()
    }

    readonly terrain: Feature<Region>
    readonly settlement: Feature<Region>
    readonly roads: Feature<Region>
    readonly forifications: Feature<Region>
    readonly trade: Feature<Region>
    readonly ship: Feature<Region>
    readonly baloon: Feature<Region>
    readonly lair: Feature<Region>
    readonly troops: Feature<Region>
    readonly onGuard: Feature<Region>
    readonly gate: Feature<Region>

    active: boolean = false
    readonly position: Point = new Point()

    private getPos(x: number = 0, y: number = 0): IPointData {
        return {
            x: this.position.x + x,
            y: this.position.y + y
        }
    }

    update() {
        this.terrain.update(this.region, this.layers, this.res)
        this.settlement.update(this.region, this.layers, this.res)
        this.roads.update(this.region, this.layers, this.res)
        this.forifications.update(this.region, this.layers, this.res)
        this.trade.update(this.region, this.layers, this.res)
        this.ship.update(this.region, this.layers, this.res)
        this.baloon.update(this.region, this.layers, this.res)
        this.lair.update(this.region, this.layers, this.res)
        this.troops.update(this.region, this.layers, this.res)
        this.onGuard.update(this.region, this.layers, this.res)
        this.gate.update(this.region, this.layers, this.res)
    }

    destroy() {
        this.terrain.destroy()
        this.settlement.destroy()
        this.roads.destroy()
        this.forifications.destroy()
        this.trade.destroy()
        this.ship.destroy()
        this.baloon.destroy()
        this.lair.destroy()
        this.troops.destroy()
        this.onGuard.destroy()
        this.gate.destroy()
    }
}

function addTestData(map: HexMap2) {
    const ruleset = new Ruleset()
    ruleset.load(rulesetData)

    const world = new World({
        map: [
            { label: 'nexus', width: 1, height: 1 },
            { label: 'surface', width: 64, height: 64 }
        ]
    }, ruleset)

    world.addFaction(report.faction.number, report.faction.name, true)

    report.attitudes.hostile
        .concat(report.attitudes.hostile)
        .concat(report.attitudes.unfriendly)
        .concat(report.attitudes.neutral)
        .concat(report.attitudes.friendly)
        .concat(report.attitudes.ally)
        .forEach(({ name, number }) => world.addFaction(number, name, false))

    for (const reg of report.regions) {
        Object.assign(reg, reg.coords)
        reg.produces = reg.products
        reg.structures = reg.structures ?? []
        reg.units = reg.units ?? []

        for (const exit of reg.exits) {
            Object.assign(exit, exit.coords)
        }

        for (const str of reg.structures) {
            Object.assign(str, str.structure)
            Object.assign(str, reg.coords)

            str.units = str.units ?? []

            for (const unit of str.units) {
                Object.assign(unit, reg.coords)
                unit.canStudy = unit.canStudy ?? []

                if (unit.faction) {
                    world.addFaction(unit.faction.number, unit.faction.name, false)
                    unit.factionNumber = unit.faction.number
                }
            }
        }

        for (const unit of reg.units) {
            Object.assign(unit, reg.coords)
            unit.canStudy = unit.canStudy ?? []

            if (unit.faction) {
                world.addFaction(unit.faction.number, unit.faction.name, false)
                unit.factionNumber = unit.faction.number
            }
        }
    }

    world.addRegions(report.regions as any)

    for (const reg of report.regions) {
        const r = world.getRegion(reg.coords)
        if (r.coords.x === 12 && r.coords.y === 30) {
            r.gate = 1
        }

        for (const unit of reg.units) {
            if (unit.faction) {
                world.addFaction(unit.faction.number, unit.faction.name, false)
            }

            const u = Unit.from(unit, world.factions, ruleset)
            r.addUnit(u)
        }

        for (const str of reg.structures) {
            const s = r.structures.find(x => x.num == str.structure.number)

            for (const unit of str.units) {
                if (unit.faction) {
                    world.addFaction(unit.faction.number, unit.faction.name, false)
                }

                const u = Unit.from(unit, world.factions, ruleset)
                r.addUnit(u, s)
            }
        }
    }

    console.log(world)

    const regs = Array.from(world.getLevel(1))
    map.setRegions(regs)
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

        // this.layout = new Layout({ x: TILE_W / 2, y: TILE_H / 2 }, { x: -100, y: -400 })
    }

    readonly renderer: AbstractRenderer
    readonly resources: Resources
    // readonly layout: Layout

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
            const p = {
                x: reg.coords.x * (TILE_W * 3 / 4),
                y: reg.coords.y * TILE_H / 2,
            }

            this.tiles.push(new Tile(p, reg, this.layers, this.resources))
        }

        this.layers.sort()
    }

    render() {
        requestAnimationFrame(() => {
            this.scene.position.set(-200, -600)
            this.renderer.render(this.scene)

            // this.scene.position.set(this.scene.width - 13, 0)
            // this.renderer.render(this.scene, { clear: false })
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
