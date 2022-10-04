import { Loader, Sprite, Texture } from 'pixi.js'
import { TileSet } from './tail-set'

export class Resources {
    constructor(private loader: Loader) {

    }

    private readonly cache = new Map<string, Texture>()
    private tileSet: TileSet

    sprite(name: string): Sprite {
        const s = new Sprite(this.texture(name))
        s.anchor.set(0.5, 0.5)

        return s
    }

    texture(name: string): Texture | null {
        if (this.cache.has(name)) {
            return this.cache.get(name)
        }

        if (name.includes('/')) {
            const [ key, spriteName ] = name.split('/')

            const sprites = this.loader.resources[key];
            const sheet = sprites.spritesheet

            return sheet.textures[spriteName] ?? sheet.textures[`${spriteName}.png`]
        }

        return this.loader.resources[name]?.texture
    }

    tile(name: string, hash: number): Sprite {
        return this.tileSet.mainTile(name, hash)
    }

    add(name: string, texture: Texture) {
        this.cache.set(name, texture)
    }

    private queueSpritesheet(name: string, url: string) {
        return new Promise((resolve, reject) => {
            if (this.loader.resources[name]) {
                resolve(this.loader.resources[name])
                return
            }

            this.loader.add(name, url, res => {
                if (res) {
                    this.tileSet = new TileSet(this.loader.resources['sprites'].spritesheet)

                    resolve(res)
                }
                else {
                    reject()
                }
            })
        })
    }

    private queueFont(name: string, size: string = '12px') {
        const fontFamily = `${size} ${name}`
        if (!document.fonts.check(fontFamily)) {
            document.fonts.load(fontFamily)
        }
    }

    async load() {
        this.queueFont('Fira Code')
        this.queueFont('Almendra')

        const tasks = Promise.all([
            document.fonts.ready,
            this.queueSpritesheet('sprites', '/sprites.json'),
        ])

        this.loader.load()

        return tasks
    }
}
