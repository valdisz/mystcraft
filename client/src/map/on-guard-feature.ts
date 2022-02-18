import { DisplayObject, IPointData } from 'pixi.js'
import { Feature } from './feature'
import { LayerName } from './layers'
import { TileState } from './tile-state'
import { Resources } from './resources'

export class OnGuardFeature extends Feature<TileState> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    protected getKey({ reg }: TileState): any[] {
        return [ reg.units.some(x => x.onGuard) ]
    }

    protected getGraphics(value: TileState, res: Resources): DisplayObject {
        if (!this.key[0]) {
            return
        }

        return res.sprite('sprites/map-guard')
    }
}
