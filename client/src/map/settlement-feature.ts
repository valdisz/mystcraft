import { DisplayObject, IPointData, Point, Text, Container } from 'pixi.js'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { TileState } from './tile-state'

export class SettlementFeature extends Feature<TileState> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    protected getKey({ reg }: TileState): any[] {
        return [ reg.settlement?.name, reg.settlement?.size ]
    }

    protected getGraphics({ reg }: TileState, res: Resources): DisplayObject {
        if (!reg.settlement?.name) {
            return
        }

        const p = new Point(12, 0)

        const text = new Text(reg.settlement.name, {
            fontSize: '16px',
            // fontFamily: 'Almendra',
            fontFamily: 'Fira Code',
            fill: 'yellow',
            fontWeight: 'bold',
            dropShadow: true,
            dropShadowColor: 'black',
            dropShadowBlur: 4,
            dropShadowDistance: 2
        })
        text.anchor.set(0, 0.5)
        text.position.copyFrom(p)

        const group = new Container()
        group.addChild(res.sprite(`sprites/map-${reg.settlement.size}`))
        group.addChild(text)

        return group
    }
}
