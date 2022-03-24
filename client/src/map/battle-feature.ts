import { DisplayObject, Graphics, Point } from 'pixi.js'
import { GlowFilter } from '@pixi/filter-glow'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { TileState } from './state'

export class BattleFeature extends Feature<TileState> {
    constructor(layer: LayerName) {
        super(layer)
    }

    protected getKey({ reg }: TileState): any[] {
        return reg.battles.length
            ? [ reg.battles.length ]
            : [ ]
    }

    protected getGraphics({ map }: TileState, res: Resources): DisplayObject {
        if (!this.key.length) {
            return
        }

        const scale = 1 / map.zoom
        const sprite = res.sprite('sprites/silhouette')
        sprite.scale.set(scale, scale)
        sprite.filters = [
            // good for showing battles or assasinations
            new GlowFilter({ distance: 32, outerStrength: 0, innerStrength: 1, quality: 0.1, color: 0xff0000, knockout: true })
        ]

        return sprite
    }
}
