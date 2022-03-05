import { DisplayObject, Text, Container, Point, Graphics } from 'pixi.js'
import { Unit } from '../game'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { TileState } from './state'

interface MenCount {
    own: number
    hostile: number
    unfriendly: number
    neutral: number
    friendly: number
    ally: number
}

interface PaletteColor {
    color: number
    shadow: number
    size: number
}

const PALETTE: PaletteColor[] = [
    { color: 0xff0000, shadow: 0x000000, size: 2 }, // red
    { color: 0xffff00, shadow: 0x000000, size: 1 }, // yellow
    { color: 0xffffff, shadow: 0x000000, size: 1 }, // white
    { color: 0x00ffff, shadow: 0x000000, size: 1 }, // aqua
    { color: 0x00ff00, shadow: 0x000000, size: 1 }, // lime
]

export class TroopsFeature extends Feature<TileState> {
    constructor(layer: LayerName) {
        super(layer)
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
        const { zoom } = value.map
        if (zoom > 2) {
            return
        }

        const [ own, ...other ] = this.key as number[]
        if (!own && other.every(x => x === 0)) {
            return
        }

        return zoom == 1
            ? this.zoom1(own, other)
            : this.zoom2(own, other)
    }

    private zoom1(own: number, other: number[]) {
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
                const { color: fill, shadow: dropShadowColor } = PALETTE[i]
                const s = TroopsFeature.formatOther(count)

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

    private zoom2(own: number, other: number[]) {
        const group = new Container()

        if (own) {
            const icon = new Graphics()

            // icon.beginFill(0x000000)
            // icon.drawCircle(0, 0, 3)
            // icon.endFill()

            icon.beginFill(0x00ff00)
            icon.lineStyle(1, 0x000000)
            icon.drawCircle(0, 0, 2.5)
            icon.endFill()

            icon.position.set(0, -4)

            group.addChild(icon)
        }

        const dp = { x: 7, y: 0}
        const p = new Point(-(dp.x * 2), 2);
        for (let i = 0; i < other.length; i++) {
            const count = other[i]
            if (count > 0) {
                const { color, shadow, size } = PALETTE[i]

                const icon = new Graphics()

                // icon.beginFill(shadow)
                // icon.drawCircle(0, 0, 2.5)
                // icon.endFill()

                icon.beginFill(color)
                icon.lineStyle(1, shadow)
                icon.drawCircle(0, 0, 2 * size)
                icon.endFill()

                icon.position.copyFrom(p)

                group.addChild(icon)
            }

            p.set(p.x + dp.x, p.y + dp.y)
        }

        return group
    }
}
