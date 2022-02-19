import { Container, DisplayObject, Graphics, IPointData, Point, Sprite } from 'pixi.js'
import { Hex, Layout } from '../geometry'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { TILE_H, TILE_W } from './tile'
import { TileState } from './tile-state'
import { pointHash } from './utils'

export class TerrainFeature extends Feature<TileState> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)

        this.hash = Math.abs(pointHash(position))
    }

    readonly hash: number

    protected getKey({ reg }: TileState): any[] {
        return [ reg.covered, reg.terrain?.code ]
    }

    protected getGraphics({ reg }: TileState, res: Resources): DisplayObject {
        let sprite: Sprite

        if (reg.covered) {
            sprite = res.tile('unexplored', this.hash)
            sprite.zIndex = -1
        }
        else {
            sprite = res.tile(reg.terrain.code, this.hash)

            if (!reg.isVisible) {
                if (reg.explored) {
                    // darken region when no region report
                    sprite.tint = 0xb0b0b0
                }
                else {
                    // darken region more when not explored
                    sprite.tint = 0xb0b0b0 * 0.75
                }
            }
        }

        return sprite
    }
}
