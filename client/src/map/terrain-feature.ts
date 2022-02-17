import { DisplayObject, IPointData, Sprite } from 'pixi.js'
import { Region } from '../game/region'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { pointHash } from './utils'

export class TerrainFeature extends Feature<Region> {
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
