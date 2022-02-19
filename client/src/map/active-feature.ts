import { DisplayObject, Graphics, IPointData, Point } from 'pixi.js'
import { GlowFilter } from '@pixi/filter-glow'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { corners } from './tile'
import { TileState } from './tile-state'

export class ActiveFeature extends Feature<TileState> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    protected getKey({ reg, isActive }: TileState): any[] {
        return [ isActive ]
    }

    protected getGraphics({ isActive }: TileState, res: Resources): DisplayObject {
        if (!isActive) {
            return
        }

        const g = new Graphics()
        g.beginFill(0xffffff, 1)
        g.drawPolygon(corners())
        g.endFill()
        g.filters = [
            // // good for showing battles or assasinations
            // new GlowFilter({ distance: 16, outerStrength: 1, innerStrength: 3, quality: 0.1, color: 0xff0000, knockout: true }),

            new GlowFilter({ distance: 16, outerStrength: 2, innerStrength: 2, quality: 0.2, color: 0x00ffaa, knockout: true })
         ]

        return g
    }
}
