import { DisplayObject, IPointData, Text, Container, Point } from 'pixi.js'
import { Unit } from '../game'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { TileState } from './tile-state'

interface MenCount {
    own: number
    hostile: number
    unfriendly: number
    neutral: number
    friendly: number
    ally: number
}

const PALETTE = [
    ['red', 'white'],
    ['yellow', 'black'],
    ['white', 'black'],
    ['aqua', 'black'],
    ['lime', 'black']
]

export class TroopsFeature extends Feature<TileState> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    static attitudeCount(value: MenCount, unit: Unit): MenCount {
        const next = { ...value }

        const key = unit.isPlayer ? 'own' : unit.faction.attitude.toLowerCase()
        next[key] += unit.inventory.menCount

        return next
    }

    protected getKey({ reg }: TileState): any[] {
        const { own, hostile, unfriendly, neutral, friendly, ally } = reg.units
            .reduce(
                (value, next) => TroopsFeature.attitudeCount(value, next),
                { own: 0, hostile: 0, unfriendly: 0, neutral: 0, friendly: 0, ally: 0 } as MenCount
            )

        return [ own, hostile, unfriendly, neutral, friendly, ally ]
    }

    static formatOwn(count: number) {
        if (count < 3000) return count.toString()
        if (count < 10000) return `${(count / 1000).toPrecision(1)}k`

        return `${Math.trunc(count / 1000).toFixed(0)}k`
    }

    static formatOther(count: number) {
        if (count === 0) return '..'
        if (count < 100) return count.toString()
        if (count < 1000) return `${Math.trunc(count / 100).toFixed(0)}h`

        return `${Math.trunc(count / 1000).toFixed(0)}k`
    }

    protected getGraphics(value: TileState, res: Resources): DisplayObject {
        const [ own, ...other ] = this.key as number[]
        if (!own && other.every(x => x === 0)) {
            return
        }

        const group = new Container()

        if (own) {
            const ownCountText = new Text(TroopsFeature.formatOwn(own), {
                fontSize: '13px',
                fontFamily: 'Fira Code',
                fill: 'lime',
                fontWeight: 'bold',
                dropShadow: true,
                dropShadowColor: 'black',
                dropShadowBlur: 4,
                dropShadowDistance: 2
            })
            ownCountText.anchor.set(0.5, 0.5)
            ownCountText.position.set(0, -6)

            group.addChild(ownCountText)
        }

        const dp = { x: 14, y: 0}
        const p = new Point(-(dp.x * 2), 8);
        for (let i = 0; i < other.length; i++) {
            const count = other[i]
            if (count > 0) {
                const s = TroopsFeature.formatOther(count)

                const [ fill, dropShadowColor ] = PALETTE[i]

                const otherCountText = new Text(s, {
                    fontSize: '11px',
                    fontFamily: 'Fira Code',
                    fill,
                    fontWeight: 'bold',
                    dropShadow: true,
                    dropShadowColor,
                    dropShadowBlur: 4,
                    dropShadowDistance: 2
                })
                otherCountText.anchor.set(0.5, 0.5)
                otherCountText.position.copyFrom(p)

                group.addChild(otherCountText)
            }

            p.set(p.x + dp.x, p.y + dp.y)
        }

        return group
    }
}
