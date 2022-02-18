import { DisplayObject, IPointData } from 'pixi.js'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { TileState } from './tile-state'

export class GateFeature extends Feature<TileState> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    protected getKey({ reg }: TileState): any[] {
        return [ reg.gate > 0 ]
    }

    protected getGraphics({ reg }: TileState, res: Resources): DisplayObject {
        if (!this.key[0]) {
            return
        }

        return res.sprite('sprites/map-gate')
    }
}
