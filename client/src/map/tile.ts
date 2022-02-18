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
import { TileState } from './tile-state'

export const TILE_W = 94;
export const TILE_H = 82;

export class Tile implements TileState {
    public constructor(position: IPointData, public readonly reg: Region, private layers: Layers, private res: Resources) {
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

    readonly terrain: Feature<TileState>
    readonly settlement: Feature<TileState>
    readonly roads: Feature<TileState>
    readonly forifications: Feature<TileState>
    readonly trade: Feature<TileState>
    readonly ship: Feature<TileState>
    readonly baloon: Feature<TileState>
    readonly lair: Feature<TileState>
    readonly troops: Feature<TileState>
    readonly onGuard: Feature<TileState>
    readonly gate: Feature<TileState>

    active = false
    readonly position = new Point()

    private getPos(x: number = 0, y: number = 0): IPointData {
        return {
            x: this.position.x + x,
            y: this.position.y + y
        }
    }

    update() {
        this.terrain.update(this, this.layers, this.res)
        this.settlement.update(this, this.layers, this.res)
        this.roads.update(this, this.layers, this.res)
        this.forifications.update(this, this.layers, this.res)
        this.trade.update(this, this.layers, this.res)
        this.ship.update(this, this.layers, this.res)
        this.baloon.update(this, this.layers, this.res)
        this.lair.update(this, this.layers, this.res)
        this.troops.update(this, this.layers, this.res)
        this.onGuard.update(this, this.layers, this.res)
        this.gate.update(this, this.layers, this.res)
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
