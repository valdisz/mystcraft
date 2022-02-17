import { IPointData, Point } from 'pixi.js'
import { Region } from '../game/region'
import { Feature } from './feature'
import { Layers } from './layers'
import { Resources } from './resources'
import { GateFeature } from './gate-feature'
import { OnGuardFeature } from './on-guard-feature'
import { RoadsFeature } from './roads-feature'
import { SettlementFeature } from './settlement-feature'
import { StructuresFeature } from './structures-feature'
import { TerrainFeature } from './terrain-feature'
import { TroopsFeature } from './troops-feature'

export const TILE_W = 94;
export const TILE_H = 82;

export class Tile {
    public constructor(position: IPointData, private region: Region, private layers: Layers, private res: Resources) {
        this.position.copyFrom(position)

        this.terrain = new TerrainFeature('terrain', this.getPos(0, -12))   // need to offest terrain tile
        this.roads = new RoadsFeature('roads', this.getPos(0, -12))
        this.settlement = new SettlementFeature('settlements', this.getPos(0, 0))

        this.baloon = new StructuresFeature('text', this.getPos(-18, TILE_H / 2), {
            spriteName: 'sprites/map-flying-ship',
            countPosition: 'top',
            matchStructure: str => str.type === 'flying-ship'
        })

        this.ship = new StructuresFeature('text', this.getPos(-6, TILE_H / 2), {
            spriteName: 'sprites/map-ship',
            countPosition: 'top',
            matchStructure: str => str.type === 'ship'
        })

        this.trade = new StructuresFeature('text', this.getPos(6, TILE_H / 2), {
            spriteName: 'sprites/map-building',
            countPosition: 'top',
            matchStructure: str => str.type === 'trade' || str.type === 'building'
        })

        this.forifications = new StructuresFeature( 'text', this.getPos(18, TILE_H / 2), {
            spriteName: 'sprites/map-fort',
            countPosition: 'top',
            matchStructure: str => str.type === 'fort'
        })

        this.lair = new StructuresFeature('text', this.getPos(0, 0), {
            spriteName: 'sprites/map-lair',
            hideCount: true,
            matchStructure: str => str.type === 'lair'
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

    active = false
    readonly position = new Point()

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
