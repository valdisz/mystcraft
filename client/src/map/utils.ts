import { IPointData, Rectangle } from 'pixi.js'
import { numhash } from '../lib'

export function pointHash({ x, y }: IPointData) {
    let seed = 1430287
    seed = seed * 7302013 ^ numhash(x)
    seed = seed * 7302013 ^ numhash(y)

    return seed
}

export function overlapping(r1: Rectangle, r2: Rectangle) {
    return r1.x < r2.x + r2.width
        && r1.x + r1.width > r2.x
        && r1.y < r2.y + r2.height
        && r1.height + r1.y > r2.y
}
