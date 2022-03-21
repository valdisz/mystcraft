import { DisplayObject, Sprite } from 'pixi.js'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { TileState } from './state'
import { pointHash } from './utils'

export class TerrainFeature extends Feature<TileState> {
    constructor(layer: LayerName) {
        super(layer)

    }

    protected getKey({ reg }: TileState): any[] {
        return [ reg.covered, reg.terrain?.code ]
    }

    protected getGraphics(value: TileState, res: Resources): DisplayObject {
        const reg = value.reg

        let sprite: Sprite
        const hash = Math.abs(pointHash(reg.coords))

        if (reg.covered) {
            sprite = res.tile('unexplored', hash)
            sprite.zIndex = -1
        }
        else {
            sprite = res.tile(reg.terrain.code, hash)

            if (!reg.isVisible) {
                // darken region when no region report
                sprite.tint = 0xb0b0b0

                if (!reg.explored) {
                    // darken region more when not explored
                    sprite.tint = 0x585858
                }
            }
        }

        const scale = 1 / value.map.zoom
        sprite.scale.set(scale, scale)

        return sprite
    }
}
