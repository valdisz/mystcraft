import { DisplayObject, IPointData, Text, Container } from 'pixi.js'
import { Region } from '../game/region'
import { Structure } from '../game/structure'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'

export interface StructuresFeatureOptions {
    spriteName: string
    countPosition?: 'top' | 'bottom'
    hideCount?: boolean
    matchStructure: (str: Structure) => boolean
}

export class StructuresFeature extends Feature<Region> {
    constructor(layer: LayerName, position: IPointData, private readonly options: StructuresFeatureOptions) {
        super(layer, position)
    }

    protected getKey(reg: Region): any[] {
        if (reg.covered) return [ ]
        return reg.structures.filter(x => this.options.matchStructure(x))
        // return [ 1 ]
    }

    protected getGraphics(reg: Region, res: Resources): DisplayObject {
        if (!this.key.length) {
            return
        }

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
                icon.position.set(0, -6)
                count.position.set(0, -16)
            }
            else {
                count.position.set(0, -6)
                icon.position.set(0, -16)
            }

            const bg = res.sprite('sprites/map-bg-1')
            bg.alpha = 0.67
            bg.position.copyFrom(count)

            group.addChild(bg)
            group.addChild(count)
            group.sortChildren()
        }

        return group
    }
}
