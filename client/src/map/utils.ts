import { IPointData } from 'pixi.js'

export function arrayEquals(a: any[], b: any[]) {
    return a.length === b.length && a.every((v, i) => v === b[i])
}

export function numhash(value: number) {
    value = ((value >> 16) ^ value) * 0x45d9f3b
    value = ((value >> 16) ^ value) * 0x45d9f3b
    value = (value >> 16) ^ value

    return value
}

export function pointHash({ x, y }: IPointData) {
    let seed = 1430287
    seed = seed * 7302013 ^ numhash(x)
    seed = seed * 7302013 ^ numhash(y)

    return seed
}
