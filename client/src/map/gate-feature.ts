import { DisplayObject, IPointData } from 'pixi.js'
import { Region } from '../game/region'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'

export class GateFeature extends Feature<Region> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    protected getKey(reg: Region): any[] {
        return [ reg.gate > 0 ]
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        if (!this.key[0]) {
            return
        }

        return res.sprite('sprites/map-gate')
    }
}
