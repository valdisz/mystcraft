import { DisplayObject, IPointData, Point } from 'pixi.js'
import { LayerName, Layers } from './layers'
import { Resources } from './resources'
import { arrayEquals } from './utils'

export abstract class Feature<T = any> {
    constructor(protected readonly layer: LayerName) {
    }

    private _key: any[] = []
    private _graphics: DisplayObject = null

    get key() { return this._key }
    get graphics() { return this._graphics }

    readonly position = new Point()

    update(value: T, layers: Layers, res: Resources, { x, y }: IPointData) {
        const posUpdated = this.position.x != x || this.position.y != y
        if (posUpdated) {
            this.position.set(x, y)
        }

        const key = this.getKey(value)
        if (!posUpdated && arrayEquals(this._key, key)) {
            return
        }

        this._key = key
        if (this._graphics) {
            layers[this.layer].removeChild(this._graphics)
            this._graphics.destroy()
        }

        this._graphics = this.getGraphics(value, res)
        if (this._graphics) {
            this._graphics.position.copyFrom(this.position)
            layers.add(this.layer, this._graphics)
        }
    }

    destroy() {
        if (this._graphics) {
            this._graphics.destroy()
        }
    }

    protected abstract getKey(value: T): any[]
    protected abstract getGraphics(value: T, res: Resources): DisplayObject | null
}
