import { ISpritesheetData, Sprite, Spritesheet } from 'pixi.js'

export interface TailSetSpec {
    main: number
}

export interface TailSetManifest extends ISpritesheetData {
    tiles: {
        [name: string]: TailSetSpec
    }
}

export class TileSet {
    constructor(private readonly spritesheet: Spritesheet) {

    }

    mainTile(name: string, hash: number) {
        if (name === 'ocean') {
            name = 'water'
        }

        const { tiles } = this.spritesheet.data as TailSetManifest
        const frameCount = tiles[name].main
        const index = hash % frameCount

        const frameName = `${name}-main-${index}`
        const texture = this.spritesheet.textures[frameName]

        const sprite = new Sprite(texture)
        sprite.anchor.set(0.5, 0.5)

        return sprite
    }
}
