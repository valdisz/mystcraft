export class Capacity {
    walk: number = 0
    ride: number = 0
    fly: number = 0
    swim: number = 0
}

export type MoveType = keyof Capacity;
