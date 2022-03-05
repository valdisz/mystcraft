import { Container, DisplayObject } from 'pixi.js'

export interface ILayers {
    readonly terrain: Container
    readonly roads: Container
    readonly highlight: Container
    readonly path: Container
    readonly features: Container
    readonly settlements: Container
    readonly text: Container
}

export class Layers implements ILayers {
    readonly terrain = new Container()
    readonly roads = new Container()
    readonly highlight = new Container()
    readonly path = new Container()
    readonly settlements = new Container()
    readonly features = new Container()
    readonly text = new Container()

    add(layer: LayerName, o: DisplayObject) {
        this[layer].addChild(o)
    }

    clearAll() {
        this.terrain.children.splice(0)
        this.roads.children.splice(0)
        this.highlight.children.splice(0)
        this.path.children.splice(0)
        this.settlements.children.splice(0)
        this.text.children.splice(0)
    }

    sort() {
        this.terrain.sortChildren()
        this.roads.sortChildren()
        this.highlight.sortChildren()
        this.path.sortChildren()
        this.settlements.sortChildren()
        this.text.sortChildren()
    }
}

export type LayerName = keyof ILayers
