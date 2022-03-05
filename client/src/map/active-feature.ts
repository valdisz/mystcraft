import { DisplayObject, Graphics, Point } from 'pixi.js'
import { GlowFilter } from '@pixi/filter-glow'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { TileState } from './state'

export class ActiveFeature extends Feature<TileState> {
    constructor(layer: LayerName) {
        super(layer)
    }

    protected getKey({ reg, isActive }: TileState): any[] {
        return [ isActive ]
    }

    protected getGraphics({ map, isActive }: TileState, res: Resources): DisplayObject {
        if (!isActive) {
            return
        }

        const scale = 1 / map.zoom
        const sprite = res.sprite('sprites/silhouette')
        sprite.scale.set(scale, scale)
        sprite.filters = [
            // // good for showing battles or assasinations
            // new GlowFilter({ distance: 16, outerStrength: 1, innerStrength: 3, quality: 0.1, color: 0xff0000, knockout: true }),

            new GlowFilter({ distance: 16 * scale, outerStrength: 2, innerStrength: 2, quality: 0.2, color: 0x00ffaa, knockout: true })
        ]

        return sprite
    }
}
