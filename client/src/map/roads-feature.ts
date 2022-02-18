import { DisplayObject, IPointData } from 'pixi.js'
import { Feature } from './feature'
import { LayerName } from './layers'
import { Resources } from './resources'
import { TileState } from './tile-state'

export class RoadsFeature extends Feature<TileState> {
    constructor(layer: LayerName, position: IPointData) {
        super(layer, position)
    }

    protected getKey({ reg }: TileState): any[] {
        return reg.structures.filter(x => x.type.startsWith('Road ')).map(x => x.type)
    }

    protected getGraphics(value: TileState, res: Resources): DisplayObject {
        if (!this.key.length) {
            return
        }

        const roads: string[] = []
        if (this.key.includes('Road N')) roads.push('n')
        if (this.key.includes('Road NW')) roads.push('nw')
        if (this.key.includes('Road NE')) roads.push('ne')
        if (this.key.includes('Road S')) roads.push('s')
        if (this.key.includes('Road SW')) roads.push('sw')
        if (this.key.includes('Road SE')) roads.push('se')

        if (!roads.length) {
            return null
        }

        const spriteName = `sprites/road-${roads.join('-')}`
        const sprite = res.sprite(spriteName)
        sprite.tint = 0x666666

        return sprite
    }
}
