import { DisplayObject, IPointData, Point, Text, Container } from 'pixi.js'
import { Region } from '../game/region'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'

interface MenCount {
    own: number
    hostile: number
    unfriendly: number
    neutral: number
    friendly: number
    ally: number
}

export class TroopsFeature extends Feature<Region> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    static attitudeCount(value: MenCount, unit: Unit): MenCount {
        const next = { ...value }

        const key = unit.isPlayer ? 'own' : unit.faction.stance.toLowerCase()
        next[key] += unit.inventory.menCount

        return next
    }

    protected getKey(reg: Region): any[] {
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
        if (count === 0) return '  '
        if (count < 100) return count.toString()
        if (count < 1000) return `${Math.trunc(count / 100).toFixed(0)}h`

        return `${Math.trunc(count / 1000).toFixed(0)}k`
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        const [ own, ...other ] = this.key as number[]
        if (!own && other.every(x => x === 0)) {
            return
        }

        const group = new Container()

        if (own) {
            const ownCountText = new Text(TroopsFeature.formatOwn(own), {
                fontSize: '10px',
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

        if (other.some(x => x !== 0)) {
            const s = other.map(x => TroopsFeature.formatOther(x)).join(' ')
            const otherCountText = new Text(s, {
                fontSize: '8px',
                fontFamily: 'Fira Code',
                fill: 'yellow',
                fontWeight: 'bold',
                dropShadow: true,
                dropShadowColor: 'black',
                dropShadowBlur: 4,
                dropShadowDistance: 2
            })
            otherCountText.anchor.set(0.5, 0.5)
            otherCountText.position.set(0, 6)

            group.addChild(otherCountText)
        }

        return group
    }
}
