import { DisplayObject, IPointData } from 'pixi.js'
import { Region } from '../game/region'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'

export class OnGuardFeature extends Feature<Region> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    protected getKey(reg: Region): any[] {
        return [ reg.units.some(x => x.onGuard) ]
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        if (!this.key[0]) {
            return
        }

        return res.sprite('sprites/map-guard')
    }
}
