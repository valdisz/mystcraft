import { DisplayObject, Point, Text, Container } from 'pixi.js'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { TileState } from './state'

const VALUES = {
    1: {
        fontSize: '16px',
        fontWeight: 'bold'
    },
    2: {
        fontSize: '14px',
        fontWeight: 'bold'
    },
    4: {
        fontSize: '12px',
        fontWeight: 'normal'
    },
}

export class SettlementFeature extends Feature<TileState> {
    constructor(layer: LayerName) {
        super(layer)
    }

    protected getKey({ reg }: TileState): any[] {
        return [ reg.settlement?.name, reg.settlement?.size ]
    }

    protected getGraphics({ map, reg }: TileState, res: Resources): DisplayObject {
        if (!reg.settlement?.name) {
            return
        }

        const p = new Point(12, 0)

        const text = new Text(reg.settlement.name, {
            ...VALUES[map.zoom],
            fontFamily: 'Fira Code',
            fill: 'white',
            dropShadow: true,
            dropShadowColor: 'black',
            dropShadowBlur: 4,
            dropShadowDistance: 3
        })
        text.anchor.set(0, 0.45)
        text.position.copyFrom(p)

        const scale = Math.min(1, 2 / map.zoom)
        const icon = res.sprite(`sprites/map-${reg.settlement.size}`)
        icon.position.set(0, 0)
        icon.scale.set(scale, scale)

        const group = new Container()
        group.addChild(icon)
        group.addChild(text)

        return group
    }
}
