import { autoDetectRenderer, Container, Loader, AbstractRenderer, Point, Rectangle, IPointData } from 'pixi.js'
import { Region } from "../game/region"

import rulesetData from './ruleset.yaml'
import report from './report.json'
import { Ruleset } from '../game/ruleset'
import { World } from '../game/world'
import { Unit } from '../game/unit'

import { Layers } from './layers'
import { Resources } from './resources'
import { Tile, TILE_H, TILE_W } from './tile'
import { Viewport } from './viewport'
import { overlapping } from './utils'
import { Layout } from '../geometry'
import { ICoords } from '../game/coords'

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
        reg.explored = true

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

export interface HexMapOptions {
    onClick?: (reg: Region) => void
    onDblClick?: (reg: Region) => void
}

export class HexMap2 {
    constructor(private canvas: HTMLCanvasElement, private readonly mapWidth: number, private readonly mapHeight: number,
        private readonly options?: HexMapOptions
    ) {
        const origin = { x: 50, y: 50 }
        this.layout = new Layout({ x: 48, y: 48 }, origin)
        const wh = this.toPixel({ x: mapWidth - 1, y: mapHeight - 1, z: 0 })

        this.viewport = new Viewport(this.canvas, origin, wh.x + 48 / 3, wh.y,
            vp => { this.render() },
            (e, vp) => {
                const tile = this.getTileAtPixel(e.clientX, e.clientY)
                if (tile) {
                    if (options?.onClick) {
                        options.onClick(tile.reg)
                    }
                }
            }
        )

        this.canvas.addEventListener('dblclick', this.onDblClick)

        this.renderer = autoDetectRenderer({
            width: this.viewport.width,
            height: this.viewport.height,
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
        this.scene.addChild(this.layers.highlight)
        this.scene.addChild(this.layers.settlements)
        this.scene.addChild(this.layers.text)
    }

    readonly viewport: Viewport
    readonly layout: Layout
    readonly renderer: AbstractRenderer
    readonly resources: Resources

    readonly scene: Container
    readonly layers: Layers

    private readonly index: number[] = []
    readonly tiles: Tile[] = []

    selectedTile: Tile

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
        const p = this.toPixel(coords)
        this.viewport.updateBounds(-(p.x - this.viewport.width / 2), -(p.y - this.viewport.height / 2))
    }

    private onDblClick = (e: MouseEvent) => {
        const tile = this.getTileAtPixel(e.clientX, e.clientY)
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

    load() {
        return this.resources
            .load()
            .then(() => {
                this.clearAll()
                addTestData(this)
            })
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

            const t = new Tile(p, reg, this.layers, this.resources)

            this.index[this.getTileIndex(reg.coords.x, reg.coords.y)] = this.tiles.length
            this.tiles.push(t)
            t.update()
        }

        this.layers.sort()
    }

    render() {
        requestAnimationFrame(() => {
            const { x, y } = this.viewport.origin

            this.scene.position.set(x, y)
            this.updateVisibility()
            this.renderer.render(this.scene)

            this.scene.position.set(x + this.viewport.mapWidth, y)
            this.updateVisibility()
            this.renderer.render(this.scene, { clear: false })

            this.scene.position.set(x - this.viewport.mapWidth, y)
            this.updateVisibility()
            this.renderer.render(this.scene, { clear: false })
        })
    }

    updateVisibility() {
        const p = new Point()

        const viewRect = this.viewport.rect
        const r = new Rectangle()

        for (const layer of this.scene.children as Container[]) {
            for (const o of layer.children as Container[]) {
                o.toGlobal(this.viewport.origin, p)

                r.x = p.x - TILE_W
                r.y = p.y - TILE_H
                r.width = o.width + TILE_W
                r.height = o.height + TILE_H

                o.visible = overlapping(viewRect, r)
            }
        }
    }

    resize(width: number, height: number) {
        this.renderer.resize(width, height)
    }

    destroy() {
        this.clearAll()

        this.renderer.destroy()
        this.canvas.removeEventListener('dblclick', this.onDblClick)
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
