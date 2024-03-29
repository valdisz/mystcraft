import { DisplayObject, Text, Container } from 'pixi.js'
import { Structure } from '../game'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { TileState } from './state'

export interface StructuresFeatureOptions {
    spriteName: string
    countPosition?: 'top' | 'bottom'
    hideCount?: boolean
    matchStructure: (str: Structure) => boolean
}

export class StructuresFeature extends Feature<TileState> {
    constructor(layer: LayerName, private readonly options: StructuresFeatureOptions) {
        super(layer)
    }

    protected getKey({ reg }: TileState): any[] {
        if (reg.covered) return [ ]
        return reg.structures.filter(x => this.options.matchStructure(x))
        // return [ 1 ]
    }

    protected getGraphics(value: TileState, res: Resources): DisplayObject {
        if (!this.key.length) {
            return
        }

        const zoom = value.map.zoom

        const group = new Container()
        const icon = res.sprite(this.options.spriteName)
        icon.zIndex = 1
        group.addChild(icon)

        if (!this.options.hideCount) {
            const countStr = this.key.length > 9
                ? '9+'
                : this.key.length.toString()

            const count = new Text(countStr, {
                fontSize: '10px',
                fontFamily: 'Fira Code',
                fill: 'black',
                fontWeight: 'bold'
            })
            count.anchor.set(0.5, 0.5)

            if (this.options.countPosition === 'top') {
                icon.position.set(0, -6 / zoom)
                count.position.set(0, -16)
            }
            else {
                count.position.set(0, -6 / zoom)
                icon.position.set(0, -16 / zoom)
            }

            const bg = res.sprite('sprites/map-bg-1')
            bg.alpha = 0.67
            bg.position.copyFrom(count)

            group.addChild(bg)
            if (zoom === 1) {
                group.addChild(count)
            }
            group.sortChildren()
        }

        return group
    }
}
