import { DisplayObject, Container } from 'pixi.js'
import { Feature } from './feature'
import { LayerName } from './layers'
import { TileState } from './state'
import { Resources } from './resources'
import { Direction, Stance } from '../schema'

interface PaletteColor {
    attitude: Stance
    color: number
}

const PALETTE: PaletteColor[] = [
    { attitude: Stance.Hostile,    color: 0xff0000 }, // red
    { attitude: Stance.Unfriendly, color: 0xffff00 }, // yellow
    { attitude: Stance.Neutral,    color: 0xffffff }, // white
    { attitude: Stance.Friendly,   color: 0x00ffff }, // aqua
    { attitude: Stance.Ally,       color: 0x00ff00 }, // lime
]

function getColor(attitude: Stance) {
    return PALETTE.find(x => x.attitude === attitude).color
}

const DIR_INDEX = {
    [ Direction.North ] : 0,
    [ Direction.Northwest ] : 1,
    [ Direction.Northeast ] : 2,
    [ Direction.South ] : 3,
    [ Direction.Southwest ] : 4,
    [ Direction.Southeast ] : 5
}

const DIR_INDEX_NAME = [
    'n', 'nw', 'ne', 's', 'sw', 'se'
]

export class OnGuardFeature extends Feature<TileState> {
    constructor(layer: LayerName) {
        super(layer)
    }

    protected getKey({ reg }: TileState): any[] {
        const guards = reg.onGuard
        const unit = guards.length > 0 ? guards[0] : null

        if (!unit) {
            return [ ]
        }

        const borders = [
            false, false, false, false, false, false
        ]

        for (const n of reg.neighbors) {
            const own = n.target.onGuard.some(x => x.faction.known && x.faction === unit.faction)
            borders[DIR_INDEX[n.direction]] = !own
        }

        return borders
    }

    protected getGraphics({ reg, map }: TileState, res: Resources): DisplayObject {
        if (map.zoom > 2) {
            return
        }

        const guards = reg.onGuard
        const unit = guards.length > 0 ? guards[0] : null

        if (!unit) {
            return
        }

        const scale = 1 / map.zoom
        const color = getColor( unit.faction.isPlayer ? Stance.Ally : unit.faction.attitude)

        const g = new Container()

        for (let i = 0; i < this.key.length; i++) {
            if (!this.key[i]) {
                continue
            }

            const sprite = res.sprite(`sprites/border-${DIR_INDEX_NAME[i]}`)
            sprite.scale.set(scale, scale)
            sprite.tint = color

            g.addChild(sprite)
        }

        return g
    }
}
